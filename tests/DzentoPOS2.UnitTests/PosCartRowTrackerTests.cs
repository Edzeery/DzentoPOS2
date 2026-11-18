using System;
using System.Collections.Generic;
using DzentoPOS2.App.Services.Sales;
using DzentoPOS2.Shared.Contracts.DTOs.Sales;
using Xunit;

namespace DzentoPOS2.UnitTests;

public sealed class PosCartRowTrackerTests
{
    [Fact]
    public void FirstApply_RequestsFullResetAndReportsEveryRow()
    {
        var tracker = new PosCartRowTracker();
        var cart = new List<CreateSaleItemDto> { Line(quantity: 1), Line(quantity: 2) };

        PosCartRowDiff diff = tracker.Apply(cart);

        Assert.True(diff.FullReset);
        Assert.Equal(new[] { 0, 1 }, diff.AddedRows);
        Assert.Empty(diff.RemovedRows);
        Assert.Empty(diff.ChangedRows);
        Assert.Equal(2, tracker.Count);
    }

    [Fact]
    public void EmptyCartOnEmptyTracker_HasNoChanges()
    {
        var tracker = new PosCartRowTracker();

        PosCartRowDiff diff = tracker.Apply(Array.Empty<CreateSaleItemDto>());

        Assert.False(diff.HasChanges);
        Assert.False(diff.FullReset);
        Assert.Equal(0, tracker.Count);
    }

    [Fact]
    public void SameCartAgain_ReturnsNoChanges()
    {
        var tracker = new PosCartRowTracker();
        var cart = new List<CreateSaleItemDto> { Line(quantity: 1), Line(quantity: 2) };
        tracker.Apply(cart);

        PosCartRowDiff diff = tracker.Apply(cart);

        Assert.False(diff.HasChanges);
        Assert.Empty(diff.AddedRows);
        Assert.Empty(diff.RemovedRows);
        Assert.Empty(diff.ChangedRows);
        Assert.False(diff.OrderChanged);
    }

    [Fact]
    public void AppendedLine_AddsOnlyTheNewRow()
    {
        var tracker = new PosCartRowTracker();
        var cart = new List<CreateSaleItemDto> { Line(quantity: 1) };
        tracker.Apply(cart);

        cart.Add(Line(quantity: 3));
        PosCartRowDiff diff = tracker.Apply(cart);

        Assert.False(diff.FullReset);
        Assert.Equal(new[] { 1 }, diff.AddedRows);
        Assert.Empty(diff.RemovedRows);
        Assert.Empty(diff.ChangedRows);
        Assert.False(diff.OrderChanged);
    }

    [Fact]
    public void QuantityChange_ReportsOnlyTheChangedRow()
    {
        var tracker = new PosCartRowTracker();
        var cart = new List<CreateSaleItemDto> { Line(quantity: 1), Line(quantity: 2) };
        tracker.Apply(cart);

        cart[1].Quantity = 5;
        PosCartRowDiff diff = tracker.Apply(cart);

        Assert.False(diff.FullReset);
        Assert.Empty(diff.AddedRows);
        Assert.Empty(diff.RemovedRows);
        Assert.Equal(new[] { 1 }, diff.ChangedRows);
        Assert.Equal(5, tracker.Rows[1].Quantity);
    }

    [Fact]
    public void PriceChange_ReportsOnlyTheChangedRow()
    {
        var tracker = new PosCartRowTracker();
        var cart = new List<CreateSaleItemDto> { Line(quantity: 1, price: 10m) };
        tracker.Apply(cart);

        cart[0].Price = 12.5m;
        PosCartRowDiff diff = tracker.Apply(cart);

        Assert.Equal(new[] { 0 }, diff.ChangedRows);
        Assert.Equal(12.5m, tracker.Rows[0].Price);
    }

    [Fact]
    public void MergedQuantity_IsReportedAsChangeNotAsNewRow()
    {
        var tracker = new PosCartRowTracker();
        var productId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        var cart = new List<CreateSaleItemDto>();
        PosCartMerger.AddOrMerge(cart, Line(productId: productId, variantId: variantId, quantity: 1));
        tracker.Apply(cart);

        PosCartMerger.AddOrMerge(cart, Line(productId: productId, variantId: variantId, quantity: 2));
        PosCartRowDiff diff = tracker.Apply(cart);

        Assert.Single(tracker.Rows);
        Assert.Empty(diff.AddedRows);
        Assert.Equal(new[] { 0 }, diff.ChangedRows);
        Assert.Equal(3, tracker.Rows[0].Quantity);
    }

    [Fact]
    public void RemoveMiddleLine_ReportsRemovedIndexAndKeepsTheRest()
    {
        var tracker = new PosCartRowTracker();
        var first = Line(quantity: 1);
        var second = Line(quantity: 2);
        var third = Line(quantity: 3);
        var cart = new List<CreateSaleItemDto> { first, second, third };
        tracker.Apply(cart);

        cart.RemoveAt(1);
        PosCartRowDiff diff = tracker.Apply(cart);

        Assert.False(diff.FullReset);
        Assert.Empty(diff.AddedRows);
        Assert.Equal(new[] { 1 }, diff.RemovedRows);
        Assert.Empty(diff.ChangedRows);
        Assert.False(diff.OrderChanged);
        Assert.Equal(2, tracker.Count);
        Assert.Equal(PosCartRowKey.From(third), tracker.Rows[1].Key);
    }

    [Fact]
    public void Reorder_ReportsOrderChanged()
    {
        var tracker = new PosCartRowTracker();
        var first = Line(quantity: 1);
        var second = Line(quantity: 2);
        var cart = new List<CreateSaleItemDto> { first, second };
        tracker.Apply(cart);

        cart.Reverse();
        PosCartRowDiff diff = tracker.Apply(cart);

        Assert.False(diff.FullReset);
        Assert.True(diff.OrderChanged);
        Assert.True(diff.HasChanges);
    }

    [Fact]
    public void ClearingTheCart_RemovesEveryPreviousRow()
    {
        var tracker = new PosCartRowTracker();
        var cart = new List<CreateSaleItemDto> { Line(quantity: 1), Line(quantity: 2) };
        tracker.Apply(cart);

        cart.Clear();
        PosCartRowDiff diff = tracker.Apply(cart);

        Assert.False(diff.FullReset);
        Assert.Equal(new[] { 0, 1 }, diff.RemovedRows);
        Assert.Empty(diff.AddedRows);
        Assert.Empty(diff.ChangedRows);
        Assert.Equal(0, tracker.Count);
    }

    [Fact]
    public void DuplicateKeysInCart_FallBackToFullReset()
    {
        var tracker = new PosCartRowTracker();
        var productId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        tracker.Apply(new[] { Line(productId: productId, variantId: variantId, quantity: 1) });

        // Bypassing the merger produces two lines for the same key.
        var cart = new List<CreateSaleItemDto>
        {
            Line(productId: productId, variantId: variantId, quantity: 1),
            Line(productId: productId, variantId: variantId, quantity: 2)
        };
        PosCartRowDiff diff = tracker.Apply(cart);

        Assert.True(diff.FullReset);
        Assert.Equal(2, tracker.Count);
    }

    [Fact]
    public void RemoveAndAddInOneStep_ReportsBothOperations()
    {
        var tracker = new PosCartRowTracker();
        var first = Line(quantity: 1);
        var second = Line(quantity: 2);
        var cart = new List<CreateSaleItemDto> { first, second };
        tracker.Apply(cart);

        var third = Line(quantity: 4);
        cart.RemoveAt(0);
        cart.Add(third);
        PosCartRowDiff diff = tracker.Apply(cart);

        Assert.False(diff.FullReset);
        Assert.Empty(diff.ChangedRows);
        Assert.Equal(new[] { 1 }, diff.AddedRows);
        Assert.Equal(new[] { 0 }, diff.RemovedRows);
        Assert.False(diff.OrderChanged);
    }

    [Fact]
    public void Clear_ForgetsTrackedRows()
    {
        var tracker = new PosCartRowTracker();
        tracker.Apply(new[] { Line(quantity: 1) });

        tracker.Clear();

        Assert.Equal(0, tracker.Count);
        PosCartRowDiff diff = tracker.Apply(new[] { Line(quantity: 2) });
        Assert.True(diff.FullReset);
    }

    private static CreateSaleItemDto Line(
        Guid? productId = null,
        Guid? variantId = null,
        decimal quantity = 1,
        decimal price = 10m,
        Guid? batchId = null,
        Guid? serialUnitId = null) =>
        new()
        {
            ProductId = productId ?? Guid.NewGuid(),
            VariantId = variantId ?? Guid.NewGuid(),
            BatchId = batchId ?? Guid.Empty,
            SerialUnitId = serialUnitId ?? Guid.Empty,
            Quantity = quantity,
            Price = price
        };
}
