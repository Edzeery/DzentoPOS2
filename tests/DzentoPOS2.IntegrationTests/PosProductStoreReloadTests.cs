using DzentoPOS2.App.Services.Inventory;
using DzentoPOS2.App.Services.Sales;
using DzentoPOS2.App.Services.Systems;
using DzentoPOS2.Core.Entities.Inventory;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Shared.Contracts.DTOs.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DzentoPOS2.IntegrationTests;

[Collection(PosDatabaseCollection.Name)]
public sealed class PosProductStoreReloadTests
{
    private readonly PosDatabaseFixture _fixture;

    public PosProductStoreReloadTests(PosDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task SnapshotCarriesBarcodesCategoryAndStock()
    {
        await using var scope = _fixture.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var products = scope.ServiceProvider.GetRequiredService<IProductService>();
        var store = (PosProductStore)scope.ServiceProvider.GetRequiredService<IPosProductStore>();

        string sku = $"SNAP-{Guid.NewGuid():N}";
        var product = await CreateProductAsync(products, sku, stock: 7);

        var variant = await context.ProductVariants
            .AsNoTracking()
            .SingleAsync(v => v.ProductId == product.Id);

        string secondBarcode = $"BC2-{Guid.NewGuid():N}";
        context.ProductBarcodes.Add(new ProductBarcode
        {
            VariantId = variant.Id,
            Barcode = $"BC1-{Guid.NewGuid():N}",
            IsPrimary = false
        });
        context.ProductBarcodes.Add(new ProductBarcode
        {
            VariantId = variant.Id,
            Barcode = secondBarcode,
            IsPrimary = true
        });
        await context.SaveChangesAsync();

        store.Invalidate();
        var items = await store.GetAllAsync();

        PosProductItem item = Assert.Single(items, x => x.VariantId == variant.Id);
        Assert.Equal(product.Id, item.ProductId);
        Assert.Equal(sku, item.SKU);
        Assert.Equal(7m, item.Stock);
        Assert.Equal(2, item.BarcodeCount);
        Assert.Equal(secondBarcode, item.PrimaryBarcode);
        Assert.False(item.HasVariants);
        Assert.False(string.IsNullOrWhiteSpace(item.CategoryName));

        var byBarcode = await store.FindByBarcodeAsync(secondBarcode);
        Assert.Single(byBarcode);
        Assert.Equal(variant.Id, byBarcode[0].VariantId);

        var byId = await store.FindByVariantIdAsync(variant.Id);
        Assert.NotNull(byId);
        Assert.Equal(item, byId);

        Assert.NotNull(store.LastLoadTimings);
        Assert.True(store.LastLoadTimings!.TotalMs > 0);
    }

    [Fact]
    public async Task CatalogChangeDuringInFlightLoad_IsNotServed()
    {
        await using var scope = _fixture.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var products = scope.ServiceProvider.GetRequiredService<IProductService>();
        var activeBranch = scope.ServiceProvider.GetRequiredService<IActiveBranchContext>();

        var notifier = new CatalogChangeNotifier();
        var loader = new GatedLoader();
        using var store = new PosProductStore(
            context,
            notifier,
            activeBranch,
            scope.ServiceProvider,
            loader);

        // The first load is held after its query returned but before the store
        // may publish it.
        Task<IReadOnlyList<PosProductItem>> pendingLoad = store.GetAllAsync();
        await loader.WaitForSnapshotAsync();

        // Second catalog change while that first result is still in flight.
        string sku = $"RACE-{Guid.NewGuid():N}";
        var created = await CreateProductAsync(products, sku, stock: 3);
        notifier.NotifyProductChanged(created.Id);

        loader.Release();

        var items = await pendingLoad;

        // The call made before the change must still return a snapshot that
        // contains the change, because the stale result was discarded.
        Assert.Contains(items, x => x.ProductId == created.Id);
        Assert.NotNull(store.CurrentSnapshot);
        Assert.Contains(store.CurrentSnapshot!.Items, x => x.ProductId == created.Id);
    }

    private static async Task<ProductDto> CreateProductAsync(
        IProductService products,
        string sku,
        decimal stock)
    {
        var categories = await products.GetCategoriesAsync();
        var units = await products.GetUnitsAsync();

        return await products.CreateAsync(new CreateProductDto
        {
            Name = $"POS Store {sku}",
            SKU = sku,
            CategoryId = categories[0].Id,
            UnitId = units[0].Id,
            CostPrice = 4,
            SalePrice = 9,
            MinimumStock = 0,
            OpeningStock = stock,
            TrackStock = true,
            IsActive = true
        });
    }

    /// <summary>
    /// Runs the real loader, then holds the first result until the test lets
    /// go. That makes the invalidation-during-load window deterministic.
    /// </summary>
    private sealed class GatedLoader : IPosProductSnapshotLoader
    {
        private readonly IPosProductSnapshotLoader _inner = PosProductSnapshotLoader.Instance;
        private readonly SemaphoreSlim _loaded = new(0, 1);
        private readonly SemaphoreSlim _release = new(0, 1);
        private int _calls;

        public async Task<PosProductSnapshot> LoadAsync(
            AppDbContext context,
            Guid branchId,
            CancellationToken cancellationToken = default)
        {
            PosProductSnapshot snapshot = await _inner
                .LoadAsync(context, branchId, cancellationToken);

            if (Interlocked.Increment(ref _calls) == 1)
            {
                _loaded.Release();
                await _release.WaitAsync(cancellationToken);
            }

            return snapshot;
        }

        public Task WaitForSnapshotAsync() => _loaded.WaitAsync(TimeSpan.FromSeconds(30));

        public void Release() => _release.Release();
    }
}
