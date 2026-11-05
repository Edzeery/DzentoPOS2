using System;
using System.Collections.Generic;
using System.Linq;
using DzentoPOS2.Core.Services.Inventory;
using Xunit;

namespace DzentoPOS2.UnitTests;

public sealed class VariantCombinationGeneratorTests
{
    [Fact]
    public void Generate_TwoAxes_ReturnsCartesianProductInOrder()
    {
        var axes = new List<IReadOnlyList<string>>
        {
            new List<string> { "Red", "Blue" },
            new List<string> { "S", "M" }
        };

        var result = VariantCombinationGenerator.Generate(axes);

        Assert.Equal(4, result.Count);
        Assert.Equal(new[] { "Red", "S" }, result[0]);
        Assert.Equal(new[] { "Red", "M" }, result[1]);
        Assert.Equal(new[] { "Blue", "S" }, result[2]);
        Assert.Equal(new[] { "Blue", "M" }, result[3]);
    }

    [Fact]
    public void Generate_SingleAxis_ReturnsEachValue()
    {
        var axes = new List<IReadOnlyList<int>> { new List<int> { 1, 2, 3 } };

        var result = VariantCombinationGenerator.Generate(axes);

        Assert.Equal(3, result.Count);
        Assert.All(result, combo => Assert.Single(combo));
        Assert.Equal(new[] { 1, 2, 3 }, result.Select(c => c[0]));
    }

    [Fact]
    public void Generate_EmptyAxis_ProducesNoCombinations()
    {
        var axes = new List<IReadOnlyList<string>>
        {
            new List<string> { "Red" },
            new List<string>()
        };

        var result = VariantCombinationGenerator.Generate(axes);

        Assert.Empty(result);
    }

    [Fact]
    public void Generate_NoAxes_ReturnsSingleEmptyCombination()
    {
        var result = VariantCombinationGenerator.Generate<string>(Array.Empty<IReadOnlyList<string>>());

        Assert.Single(result);
        Assert.Empty(result[0]);
    }

    [Fact]
    public void Generate_Null_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            VariantCombinationGenerator.Generate<string>(null!));
    }
}
