using DzentoPOS2.App.Services.Sales;
using DzentoPOS2.Core.Services.Sales;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Shared.Contracts.DTOs.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DzentoPOS2.IntegrationTests;

[Collection(PosDatabaseCollection.Name)]
public sealed class SaleServiceE1Tests
{
    private readonly PosDatabaseFixture _fixture;

    public SaleServiceE1Tests(PosDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task SavedInvoiceTotalsMatchCalculator()
    {
        await using var scope = _fixture.CreateScope();

        var service = scope.ServiceProvider.GetRequiredService<ISaleService>();

        var dto = new CreateSaleDto
        {
            DiscountMode = DzentoPOS2.Core.Services.Sales.DiscountMode.Percent,
            DiscountValue = 10,
            TaxRate = 20,
            PaidAmount = 150,
            Items =
            {
                new CreateSaleItemDto
                {
                    ProductId = _fixture.Data.ProductId,
                    Quantity = 2,
                    Price = 50
                }
            }
        };

        var result = await service.CreateAsync(dto);

        var expected = InvoiceCalculator.Calculate(
            new[] { new InvoiceLine(2, 50) },
            DzentoPOS2.Core.Services.Sales.DiscountMode.Percent,
            10,
            20,
            150,
            2);

        Assert.Equal(expected.Subtotal, result.Subtotal);
        Assert.Equal(expected.DiscountAmount, result.DiscountAmount);
        Assert.Equal(expected.TaxAmount, result.TaxAmount);
        Assert.Equal(expected.Total, result.Total);

        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var saved = await context.SalesInvoices
            .AsNoTracking()
            .SingleAsync(x => x.Id == result.Id);

        Assert.Equal(expected.Subtotal, saved.Subtotal.Amount);
        Assert.Equal(expected.DiscountAmount, saved.DiscountAmount.Amount);
        Assert.Equal(expected.TaxAmount, saved.TaxAmount.Amount);
        Assert.Equal(expected.Total, saved.Total.Amount);
        Assert.Equal(_fixture.Data.BranchId, saved.BranchId);
    }

    [Fact]
    public async Task ParallelSalesReceiveDistinctInvoiceNumbers()
    {
        const int salesCount = 20;

        var tasks = Enumerable.Range(0, salesCount)
            .Select(async index =>
            {
                await using var scope = _fixture.CreateScope();
                var service = scope.ServiceProvider
                    .GetRequiredService<ISaleService>();

                var result = await service.CreateAsync(new CreateSaleDto
                {
                    PaidAmount = 10 + index,
                    Items =
                    {
                        new CreateSaleItemDto
                        {
                            ProductId = _fixture.Data.ProductId,
                            Quantity = 1,
                            Price = 10 + index
                        }
                    }
                });

                return result.InvoiceNumber;
            });

        var numbers = await Task.WhenAll(tasks);

        Assert.Equal(salesCount, numbers.Length);
        Assert.Equal(salesCount, numbers.Distinct().Count());
        Assert.All(numbers, number => Assert.StartsWith("INV-", number));
    }
}
