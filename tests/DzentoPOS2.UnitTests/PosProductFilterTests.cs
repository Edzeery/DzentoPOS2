using System;
using System.Collections.Generic;
using System.Linq;
using DzentoPOS2.App.Services.Sales;
using Xunit;

namespace DzentoPOS2.UnitTests;

public sealed class PosProductFilterTests
{
    private static readonly Guid FoodCategory = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid DrinkCategory = Guid.Parse("22222222-2222-2222-2222-222222222222");

    [Fact]
    public void Apply_WithoutTermOrCategory_ReturnsTheSameInstance()
    {
        var items = new List<PosProductItem> { Item("Milk", "SKU-1"), Item("Bread", "SKU-2") };

        var result = PosProductFilter.Apply(items, null);

        Assert.Same(items, result);
    }

    [Fact]
    public void Apply_WhitespaceTerm_ReturnsTheSameInstance()
    {
        var items = new List<PosProductItem> { Item("Milk", "SKU-1") };

        var result = PosProductFilter.Apply(items, "   ");

        Assert.Same(items, result);
    }

    [Fact]
    public void Apply_MatchesNameCaseInsensitive()
    {
        var milk = Item("Milk 1L", "SKU-1");
        var bread = Item("Bread", "SKU-2");

        var result = PosProductFilter.Apply(new[] { milk, bread }, "mILk");

        Assert.Equal(new[] { milk }, result);
    }

    [Fact]
    public void Apply_MatchesSku()
    {
        var milk = Item("Milk 1L", "SKU-MILK");
        var bread = Item("Bread", "SKU-BREAD");

        var result = PosProductFilter.Apply(new[] { milk, bread }, "sku-br");

        Assert.Equal(new[] { bread }, result);
    }

    [Fact]
    public void Apply_MatchesPrimaryBarcode()
    {
        var milk = Item("Milk 1L", "SKU-1", barcode: "6130000000012");
        var bread = Item("Bread", "SKU-2", barcode: "6130000000029");

        var result = PosProductFilter.Apply(new[] { milk, bread }, "0000012");

        Assert.Equal(new[] { milk }, result);
    }

    [Fact]
    public void Apply_MatchesEveryProductIdResolvedFromBarcodes()
    {
        // A product with two variants: the barcode of variant B is read from
        // the resolver and must keep variant A too.
        var variantA = Item("Milk 1L", "SKU-1", productId: Guid.Parse("aaaaaaaa-1111-1111-1111-111111111111"));
        var variantB = Item("Milk 2L", "SKU-2", productId: variantA.ProductId);
        var bread = Item("Bread", "SKU-3");

        var result = PosProductFilter.Apply(
            new[] { variantA, variantB, bread },
            "resolved-barcode",
            barcodeProductIds: new[] { variantA.ProductId });

        Assert.Equal(new[] { variantA, variantB }, result);
    }

    [Fact]
    public void Apply_CategoryOnly_KeepsProductsOfThatCategory()
    {
        var milk = Item("Milk", "SKU-1", categoryId: FoodCategory);
        var cola = Item("Cola", "SKU-2", categoryId: DrinkCategory);

        var result = PosProductFilter.Apply(new[] { milk, cola }, null, DrinkCategory);

        Assert.Equal(new[] { cola }, result);
    }

    [Fact]
    public void Apply_CombinesCategoryAndTerm()
    {
        var milk = Item("Milk", "SKU-1", categoryId: FoodCategory);
        var milkshake = Item("Milkshake", "SKU-2", categoryId: DrinkCategory);

        var result = PosProductFilter.Apply(new[] { milk, milkshake }, "milk", FoodCategory);

        Assert.Equal(new[] { milk }, result);
    }

    [Fact]
    public void Apply_EmptyCategoryGuid_MeansAllCategories()
    {
        var milk = Item("Milk", "SKU-1", categoryId: FoodCategory);
        var cola = Item("Cola", "SKU-2", categoryId: DrinkCategory);

        var result = PosProductFilter.Apply(new[] { milk, cola }, null, Guid.Empty);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void Apply_NoMatch_ReturnsEmptyList()
    {
        var result = PosProductFilter.Apply(new[] { Item("Milk", "SKU-1") }, "zzz");

        Assert.Empty(result);
    }

    [Fact]
    public void Apply_DoesNotMutateTheSource()
    {
        var items = new List<PosProductItem> { Item("Milk", "SKU-1"), Item("Bread", "SKU-2") };

        PosProductFilter.Apply(items, "milk");

        Assert.Equal(2, items.Count);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("   ", null)]
    [InlineData(" milk ", "milk")]
    public void Normalize_TrimsAndDropsEmptyTerms(string? input, string? expected)
    {
        Assert.Equal(expected, PosProductFilter.Normalize(input));
    }

    private static PosProductItem Item(
        string name,
        string? sku = null,
        string? barcode = null,
        Guid? categoryId = null,
        Guid? productId = null) =>
        new(
            VariantId: Guid.NewGuid(),
            ProductId: productId ?? Guid.NewGuid(),
            Name: name,
            SKU: sku,
            SalePrice: 10m,
            Stock: 5m,
            IsActive: true,
            HasVariants: false,
            UnitId: Guid.NewGuid(),
            UnitName: "Piece",
            CategoryId: categoryId ?? FoodCategory,
            ImagePath: null,
            ThumbnailPath: null,
            PrimaryBarcode: barcode);
}
