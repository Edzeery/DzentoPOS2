using DzentoPOS2.App;
using DzentoPOS2.App.Services.Accounting;
using DzentoPOS2.App.Services.Events;
using DzentoPOS2.App.Services.Sales;
using DzentoPOS2.App.Session;
using DzentoPOS2.Core.Entities.Accounting;
using DzentoPOS2.Core.Enums;
using DzentoPOS2.Core.Exceptions;
using DzentoPOS2.Data;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Shared.Contracts.DTOs.Accounting;
using DzentoPOS2.Shared.Contracts.DTOs.Sales;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DzentoPOS2.IntegrationTests;

/// <summary>
/// Phase E3 cash-drawer integration tests. Every test builds its own migrated
/// SQLite database so the tests do not share the single open session of
/// <see cref="PosDatabaseFixture"/>: they need full control over the drawer
/// lifecycle (open / close / no session).
/// </summary>
public sealed class CashSessionE3Tests : IAsyncLifetime
{
    private ServiceProvider _provider = null!;
    private string _dbPath = string.Empty;
    private TestData _data = null!;

    public async Task InitializeAsync()
    {
        (_provider, _dbPath) = BuildProvider();

        await using var scope = _provider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.MigrateAsync();

        _data = await TestData.SeedAsync(context);

        var currentUser = scope.ServiceProvider.GetRequiredService<ICurrentUserService>();
        currentUser.SetUser(new UserSession
        {
            UserId = _data.UserId,
            UserName = "cashier",
            FullName = "Test Cashier",
            RoleName = "Cashier",
            BranchId = _data.BranchId,
            BranchName = "Main Branch",
            IsAdmin = true
        });
    }

    public async Task DisposeAsync()
    {
        await _provider.DisposeAsync();
        SqliteConnection.ClearAllPools();

        if (File.Exists(_dbPath))
            File.Delete(_dbPath);
    }

    private AsyncServiceScope CreateScope() => _provider.CreateAsyncScope();

    private static CreateSaleDto Sale(
        Guid productId,
        string method = "Cash",
        decimal paid = 10,
        decimal price = 10,
        decimal quantity = 1,
        Guid? customerId = null)
    {
        return new CreateSaleDto
        {
            CustomerId = customerId ?? Guid.Empty,
            PaymentMethod = method,
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

    private static (ServiceProvider Provider, string DbPath) BuildProvider()
    {
        var directory = Path.Combine(Path.GetTempPath(), "DzentoPOS2.E3.Cash");
        Directory.CreateDirectory(directory);

        var dbPath = Path.Combine(directory, $"{Guid.NewGuid():N}.db");

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = $"Data Source={dbPath}"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();
        services.AddDataServices(configuration);
        services.AddApplicationServices();

        return (services.BuildServiceProvider(), dbPath);
    }

    // ── Cash sale ──────────────────────────────────────────────────────────

    [Fact]
    public async Task Cash_sale_creates_one_linked_cash_transaction_and_the_summary_reflects_it()
    {
        await using var scope = CreateScope();
        var sales = scope.ServiceProvider.GetRequiredService<ISaleService>();
        var sessions = scope.ServiceProvider.GetRequiredService<ICashSessionService>();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var open = await sessions.GetOpenSessionAsync();
        Assert.NotNull(open);

        var sale = await sales.CreateAsync(Sale(_data.ProductId, paid: 10, price: 10));

        var cashRows = await context.CashTransactions
            .AsNoTracking()
            .Where(c => c.SourceId == sale.Id && c.TransactionType == MovementType.SaleCashIn)
            .ToListAsync();

        Assert.Single(cashRows);
        Assert.Equal(open!.Id, cashRows[0].CashSessionId);
        Assert.Equal(10m, cashRows[0].Amount);

        var summary = await sessions.GetSessionSummaryAsync(open.Id);

        Assert.Equal(10m, summary.TotalSales);
        Assert.Equal(10m, summary.CashSales);
        Assert.Equal(1, summary.InvoiceCount);
        Assert.Equal(1010m, summary.ExpectedBalance);
    }

    [Fact]
    public async Task Card_sale_never_touches_the_drawer_and_shows_up_only_in_non_cash()
    {
        await using var scope = CreateScope();
        var sales = scope.ServiceProvider.GetRequiredService<ISaleService>();
        var sessions = scope.ServiceProvider.GetRequiredService<ICashSessionService>();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var open = await sessions.GetOpenSessionAsync();
        Assert.NotNull(open);

        await sales.CreateAsync(Sale(_data.ProductId, method: "Card", paid: 10, price: 10));

        Assert.False(await context.CashTransactions
            .AsNoTracking()
            .AnyAsync(c => c.TransactionType == MovementType.SaleCashIn));

        var summary = await sessions.GetSessionSummaryAsync(open!.Id);

        Assert.Equal(0m, summary.CashSales);
        Assert.Equal(10m, summary.TotalSales);
        Assert.Equal(10m, summary.NonCashSalesByMethod["Card"]);
        Assert.Equal(1000m, summary.ExpectedBalance);
    }

    [Fact]
    public async Task Partial_payment_counts_only_the_cash_that_was_actually_paid()
    {
        await using var scope = CreateScope();
        var sales = scope.ServiceProvider.GetRequiredService<ISaleService>();
        var sessions = scope.ServiceProvider.GetRequiredService<ICashSessionService>();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var customer = new DzentoPOS2.Core.Entities.Partners.Customer { FullName = "Credit Buyer" };
        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        var open = await sessions.GetOpenSessionAsync();
        Assert.NotNull(open);

        var sale = await sales.CreateAsync(
            Sale(_data.ProductId, paid: 4, price: 10, customerId: customer.Id));

        Assert.Equal(4m, sale.PaidAmount);
        Assert.Equal(6m, sale.RemainingAmount);

        var summary = await sessions.GetSessionSummaryAsync(open!.Id);

        Assert.Equal(4m, summary.CashSales);
        Assert.Equal(10m, summary.TotalSales);
        Assert.Equal(1004m, summary.ExpectedBalance);
    }

    // ── No session ─────────────────────────────────────────────────────────

    [Fact]
    public async Task Sale_without_an_open_session_is_rejected_and_persists_nothing()
    {
        await using var scope = CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Close the seeded session so the cashbox has no open drawer.
        var seeded = await context.CashSessions.FirstAsync(s => s.Status == CashSessionStatus.Open);
        seeded.Status = CashSessionStatus.Closed;
        seeded.ClosedAtUtc = DateTime.UtcNow;
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var sales = scope.ServiceProvider.GetRequiredService<ISaleService>();

        await Assert.ThrowsAsync<CashSessionRequiredException>(
            () => sales.CreateAsync(Sale(_data.ProductId, paid: 10, price: 10)));

        await using var verify = CreateScope();
        var verifyContext = verify.ServiceProvider.GetRequiredService<AppDbContext>();

        Assert.Equal(0, await verifyContext.SalesInvoices.CountAsync());
        Assert.Equal(0, await verifyContext.SalesPayments.CountAsync());
        Assert.Equal(0, await verifyContext.CashTransactions.CountAsync());
    }

    // ── One open session per cashbox ───────────────────────────────────────

    [Fact]
    public async Task Second_open_session_is_rejected_by_the_service_and_by_the_unique_index()
    {
        await using var scope = CreateScope();
        var sessions = scope.ServiceProvider.GetRequiredService<ICashSessionService>();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var ex = await Assert.ThrowsAsync<CodedDomainException>(
            () => sessions.OpenSessionAsync(new OpenCashSessionDto
            {
                CashboxId = _data.CashboxId,
                OpeningBalance = 100
            }));

        Assert.Equal(DomainErrorCodes.SessionAlreadyOpen, ex.Code);

        // Last line of defence: the filtered unique index rejects even the
        // direct insert that bypasses the service.
        context.CashSessions.Add(new CashSession
        {
            CashboxId = _data.CashboxId,
            OpenedAtUtc = DateTime.UtcNow,
            OpeningBalance = 100,
            Status = CashSessionStatus.Open
        });

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    // ── Void ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task Voiding_a_cash_sale_reverses_totals_and_cash()
    {
        await using var scope = CreateScope();
        var sales = scope.ServiceProvider.GetRequiredService<ISaleService>();
        var sessions = scope.ServiceProvider.GetRequiredService<ICashSessionService>();

        var open = await sessions.GetOpenSessionAsync();
        Assert.NotNull(open);

        var sale = await sales.CreateAsync(Sale(_data.ProductId, paid: 10, price: 10));
        await sales.VoidAsync(sale.Id, "customer returned");

        var summary = await sessions.GetSessionSummaryAsync(open!.Id);

        Assert.Equal(0m, summary.TotalSales);
        Assert.Equal(10m, summary.CashSales);
        Assert.Equal(10m, summary.CashRefunds);
        Assert.Equal(0, summary.InvoiceCount);
        Assert.Equal(1, summary.VoidCount);
        Assert.Equal(1000m, summary.ExpectedBalance);
    }

    [Fact]
    public async Task Voiding_a_card_sale_never_moves_the_drawer()
    {
        await using var scope = CreateScope();
        var sales = scope.ServiceProvider.GetRequiredService<ISaleService>();
        var sessions = scope.ServiceProvider.GetRequiredService<ICashSessionService>();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var open = await sessions.GetOpenSessionAsync();
        Assert.NotNull(open);

        var sale = await sales.CreateAsync(Sale(_data.ProductId, method: "Card", paid: 10, price: 10));
        await sales.VoidAsync(sale.Id, "customer returned");

        Assert.False(await context.CashTransactions
            .AsNoTracking()
            .AnyAsync(c => c.TransactionType == MovementType.SaleVoidCashOut));

        var summary = await sessions.GetSessionSummaryAsync(open!.Id);

        Assert.Equal(0m, summary.CashSales);
        Assert.Equal(0m, summary.CashRefunds);
        Assert.Equal(1000m, summary.ExpectedBalance);
    }

    // ── Total sales and closing ────────────────────────────────────────────

    [Fact]
    public async Task Total_sales_is_the_sum_of_non_voided_invoice_totals()
    {
        await using var scope = CreateScope();
        var sales = scope.ServiceProvider.GetRequiredService<ISaleService>();
        var sessions = scope.ServiceProvider.GetRequiredService<ICashSessionService>();

        var open = await sessions.GetOpenSessionAsync();
        Assert.NotNull(open);

        await sales.CreateAsync(Sale(_data.ProductId, paid: 10, price: 10));
        var second = await sales.CreateAsync(Sale(_data.ProductId, paid: 20, price: 20));
        await sales.CreateAsync(Sale(_data.ProductId, paid: 30, price: 30));

        await sales.VoidAsync(second.Id, "return");

        var summary = await sessions.GetSessionSummaryAsync(open!.Id);

        Assert.Equal(40m, summary.TotalSales);
        Assert.Equal(2, summary.InvoiceCount);
        Assert.Equal(1, summary.VoidCount);
    }

    [Fact]
    public async Task Close_session_stores_exactly_the_summary_expected_balance_and_cannot_close_twice()
    {
        await using var scope = CreateScope();
        var sales = scope.ServiceProvider.GetRequiredService<ISaleService>();
        var sessions = scope.ServiceProvider.GetRequiredService<ICashSessionService>();

        var open = await sessions.GetOpenSessionAsync();
        Assert.NotNull(open);

        await sales.CreateAsync(Sale(_data.ProductId, paid: 10, price: 10));

        var summary = await sessions.GetSessionSummaryAsync(open!.Id);

        var closed = await sessions.CloseSessionAsync(new CloseCashSessionDto
        {
            SessionId = open.Id,
            ClosingBalance = summary.ExpectedBalance
        });

        Assert.Equal(summary.ExpectedBalance, closed.ExpectedBalance);
        Assert.Equal(0m, closed.Difference);
        Assert.Equal(CashSessionStatus.Closed, closed.Status);

        var ex = await Assert.ThrowsAsync<CodedDomainException>(
            () => sessions.CloseSessionAsync(new CloseCashSessionDto
            {
                SessionId = open.Id,
                ClosingBalance = 0
            }));

        Assert.Equal(DomainErrorCodes.SessionAlreadyClosed, ex.Code);
    }

    // ── Events ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Sale_event_is_raised_once_after_the_commit_and_the_row_is_visible()
    {
        await using var scope = CreateScope();
        var sales = scope.ServiceProvider.GetRequiredService<ISaleService>();
        var publisher = _provider.GetRequiredService<ISalesEventPublisher>();

        var calls = 0;
        Guid raisedId = Guid.Empty;
        var visibleInsideHandler = false;

        EventHandler<SaleCompletedEventArgs> handler = (_, e) =>
        {
            calls++;
            raisedId = e.InvoiceId;

            using var verify = _provider.CreateScope();
            var verifyContext = verify.ServiceProvider.GetRequiredService<AppDbContext>();
            visibleInsideHandler = verifyContext.SalesInvoices
                .AsNoTracking()
                .Any(x => x.Id == e.InvoiceId);
        };

        publisher.SaleCompleted += handler;
        try
        {
            var sale = await sales.CreateAsync(Sale(_data.ProductId, paid: 10, price: 10));

            Assert.Equal(1, calls);
            Assert.Equal(sale.Id, raisedId);
            Assert.True(visibleInsideHandler);
        }
        finally
        {
            publisher.SaleCompleted -= handler;
        }
    }

    [Fact]
    public void A_disposed_subscriber_receives_no_further_events()
    {
        var publisher = _provider.GetRequiredService<ISalesEventPublisher>();

        var subscriber = new CountingSubscriber(publisher);
        subscriber.Dispose();

        publisher.PublishSaleCompleted(new SaleCompletedEventArgs(
            Guid.NewGuid(),
            "INV-X",
            1m,
            1m,
            "Cash",
            _data.CashboxId,
            null,
            DateTime.UtcNow));

        Assert.Equal(0, subscriber.Count);
    }

    private sealed class CountingSubscriber : IDisposable
    {
        private readonly ISalesEventPublisher _publisher;

        public CountingSubscriber(ISalesEventPublisher publisher)
        {
            _publisher = publisher;
            _publisher.SaleCompleted += OnSaleCompleted;
        }

        public int Count { get; private set; }

        private void OnSaleCompleted(object? sender, SaleCompletedEventArgs e) => Count++;

        public void Dispose() => _publisher.SaleCompleted -= OnSaleCompleted;
    }
}
