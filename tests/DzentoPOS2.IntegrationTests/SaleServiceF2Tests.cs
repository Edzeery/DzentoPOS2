using DzentoPOS2.App.Services.Sales;
using DzentoPOS2.Core.Entities.Partners;
using DzentoPOS2.Core.Entities.Systems;
using DzentoPOS2.Core.Enums;
using DzentoPOS2.Core.Exceptions;
using DzentoPOS2.Core.Services.Sales;
using DzentoPOS2.Core.Systems;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Shared.Contracts.DTOs.Sales;
using DzentoPOS2.Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DzentoPOS2.IntegrationTests;

/// <summary>
/// End-to-end discount / payment correctness: the screen preview, the stored
/// invoice, the payment row and the cash movement must be provably identical,
/// and the service (not only the UI) enforces the rules.
/// </summary>
[Collection(PosDatabaseCollection.Name)]
public sealed class SaleServiceF2Tests
{
    private readonly PosDatabaseFixture _fixture;

    public SaleServiceF2Tests(PosDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    private static CreateSaleDto Sale(
        Guid productId,
        Guid? customerId = null,
        DiscountMode mode = DiscountMode.Percent,
        decimal discount = 0,
        decimal tax = 0,
        decimal paid = 0,
        decimal quantity = 1,
        decimal price = 10)
    {
        return new CreateSaleDto
        {
            CustomerId = customerId ?? Guid.Empty,
            DiscountMode = mode,
            DiscountValue = discount,
            TaxRate = tax,
            PaidAmount = paid,
            Items =
            {
                new CreateSaleItemDto
                {
                    ProductId = productId,
                    Quantity = quantity,
                    Price = price
                }
            }
        };
    }

    [Fact]
    public async Task Overpayment_stores_net_paid_and_net_cash_and_keeps_calculator_parity()
    {
        await using var scope = _fixture.CreateScope();
        var sales = scope.ServiceProvider.GetRequiredService<ISaleService>();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var regional = scope.ServiceProvider.GetRequiredService<RegionalContext>();

        var items = new List<CreateSaleItemDto>
        {
            new() { ProductId = _fixture.Data.ProductId, Quantity = 2, Price = 50 }
        };

        var dto = new CreateSaleDto
        {
            CustomerId = Guid.Empty,
            DiscountMode = DiscountMode.Percent,
            DiscountValue = 10,
            TaxRate = 19,
            PaidAmount = 200,
            Items = items
        };

        var expected = InvoiceCalculator.Calculate(
            items.Select(i => new InvoiceLine(i.Quantity, i.Price)).ToList(),
            DiscountMode.Percent,
            10,
            19,
            200,
            regional.CurrencyDecimalDigits);

        var sale = await sales.CreateAsync(dto);

        Assert.Equal(expected.Subtotal, sale.Subtotal);
        Assert.Equal(expected.DiscountAmount, sale.DiscountAmount);
        Assert.Equal(expected.AfterDiscount, sale.AfterDiscount);
        Assert.Equal(expected.TaxAmount, sale.TaxAmount);
        Assert.Equal(expected.Total, sale.Total);
        Assert.Equal(expected.AppliedPayment, sale.PaidAmount);
        Assert.Equal(expected.Change, sale.ChangeAmount);
        Assert.Equal(expected.TenderedAmount, sale.TenderedAmount);

        var invoice = await context.SalesInvoices
            .AsNoTracking()
            .SingleAsync(x => x.Id == sale.Id);

        Assert.Equal(expected.Total, invoice.PaidAmount.Amount);
        Assert.Equal(0, invoice.RemainingAmount.Amount);

        var payment = await context.SalesPayments
            .AsNoTracking()
            .SingleAsync(p => p.SalesInvoiceId == sale.Id);

        Assert.Equal(expected.AppliedPayment, payment.Amount);
        Assert.Equal(200, payment.TenderedAmount);
        Assert.Equal(expected.Change, payment.ChangeAmount);

        var cash = await context.CashTransactions
            .AsNoTracking()
            .SingleAsync(c =>
                c.SourceId == sale.Id &&
                c.TransactionType == MovementType.SaleCashIn);

        Assert.Equal(expected.Total, cash.Amount);
    }

    [Fact]
    public async Task Fixed_discount_above_subtotal_is_rejected_by_the_service()
    {
        await using var scope = _fixture.CreateScope();
        var sales = scope.ServiceProvider.GetRequiredService<ISaleService>();

        var dto = Sale(
            _fixture.Data.ProductId,
            mode: DiscountMode.FixedAmount,
            discount: 9999,
            paid: 0,
            price: 10);

        await Assert.ThrowsAnyAsync<DomainException>(() => sales.CreateAsync(dto));
    }

    [Fact]
    public async Task Credit_sale_without_a_customer_is_rejected()
    {
        await using var scope = _fixture.CreateScope();
        var sales = scope.ServiceProvider.GetRequiredService<ISaleService>();

        var dto = Sale(_fixture.Data.ProductId, customerId: Guid.Empty, paid: 0, price: 10);

        await Assert.ThrowsAnyAsync<DomainException>(() => sales.CreateAsync(dto));
    }

    [Fact]
    public async Task Credit_sale_with_a_customer_records_the_remaining_balance()
    {
        await using var scope = _fixture.CreateScope();
        var sales = scope.ServiceProvider.GetRequiredService<ISaleService>();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var customer = new Customer { FullName = "Credit Buyer" };
        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        var sale = await sales.CreateAsync(
            Sale(_fixture.Data.ProductId, customerId: customer.Id, paid: 4, price: 10));

        Assert.Equal(10, sale.Total);
        Assert.Equal(4, sale.PaidAmount);
        Assert.Equal(6, sale.RemainingAmount);

        var invoice = await context.SalesInvoices
            .AsNoTracking()
            .SingleAsync(x => x.Id == sale.Id);

        Assert.Equal(6, invoice.RemainingAmount.Amount);

        var payment = await context.SalesPayments
            .AsNoTracking()
            .SingleAsync(p => p.SalesInvoiceId == sale.Id);

        Assert.Equal(4, payment.Amount);
        Assert.Equal(0, payment.ChangeAmount);

        var cash = await context.CashTransactions
            .AsNoTracking()
            .SingleAsync(c =>
                c.SourceId == sale.Id &&
                c.TransactionType == MovementType.SaleCashIn);

        Assert.Equal(4, cash.Amount);
    }

    [Fact]
    public async Task Voiding_reverses_exactly_the_applied_cash()
    {
        await using var scope = _fixture.CreateScope();
        var sales = scope.ServiceProvider.GetRequiredService<ISaleService>();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var sale = await sales.CreateAsync(
            Sale(_fixture.Data.ProductId, paid: 100, price: 10));

        // Paid 100 for a 10.00 total -> applied 10, change 90.
        Assert.Equal(10, sale.PaidAmount);
        Assert.Equal(90, sale.ChangeAmount);

        await sales.VoidAsync(sale.Id, "customer returned");

        var voidCash = await context.CashTransactions
            .AsNoTracking()
            .SingleAsync(c =>
                c.SourceId == sale.Id &&
                c.TransactionType == MovementType.SaleVoidCashOut);

        Assert.Equal(10, voidCash.Amount);
    }

    [Fact]
    public async Task Discount_ceiling_applies_to_fixed_mode_via_effective_percent()
    {
        await using var scope = _fixture.CreateScope();
        var sales = scope.ServiceProvider.GetRequiredService<ISaleService>();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await SetMaxDiscountAsync(context, 10);

        // subtotal 100, fixed 50 -> effective 50% > 10% -> rejected.
        await Assert.ThrowsAsync<CodedDomainException>(() => sales.CreateAsync(
            Sale(_fixture.Data.ProductId, mode: DiscountMode.FixedAmount, discount: 50, paid: 50, price: 100)));

        // fixed 10 -> effective 10% -> accepted.
        var sale = await sales.CreateAsync(
            Sale(_fixture.Data.ProductId, mode: DiscountMode.FixedAmount, discount: 10, paid: 90, price: 100));

        Assert.Equal(10, sale.DiscountAmount);
        Assert.Equal(90, sale.Total);
    }

    [Fact]
    public async Task Discount_ceiling_still_applies_to_percent_mode()
    {
        await using var scope = _fixture.CreateScope();
        var sales = scope.ServiceProvider.GetRequiredService<ISaleService>();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await SetMaxDiscountAsync(context, 10);

        await Assert.ThrowsAsync<CodedDomainException>(() => sales.CreateAsync(
            Sale(_fixture.Data.ProductId, mode: DiscountMode.Percent, discount: 50, paid: 50, price: 100)));
    }

    [Fact]
    public async Task Discount_ceiling_ignores_a_zero_subtotal()
    {
        await using var scope = _fixture.CreateScope();
        var sales = scope.ServiceProvider.GetRequiredService<ISaleService>();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await SetMaxDiscountAsync(context, 0);

        // subtotal 0 -> no effective discount -> accepted.
        var sale = await sales.CreateAsync(
            Sale(_fixture.Data.ProductId, mode: DiscountMode.FixedAmount, discount: 0, paid: 0, price: 0));

        Assert.Equal(0, sale.Total);
    }

    private static async Task SetMaxDiscountAsync(AppDbContext context, int percent)
    {
        var setting = await context.AppSettings
            .FirstOrDefaultAsync(x => x.Key == AppSettingKeys.MaxDiscountPercent);

        if (setting is null)
        {
            setting = new AppSetting { Key = AppSettingKeys.MaxDiscountPercent, Group = "Sales" };
            context.AppSettings.Add(setting);
        }

        setting.Value = percent.ToString();
        await context.SaveChangesAsync();
    }
}