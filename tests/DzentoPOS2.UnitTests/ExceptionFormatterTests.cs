using DzentoPOS2.App.Diagnostics;
using DzentoPOS2.Core.Exceptions;
using DzentoPOS2.Localization.Resources;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DzentoPOS2.UnitTests;

public sealed class ExceptionFormatterTests
{
    [Fact]
    public void Foreign_key_message_is_categorized_as_foreign_key()
    {
        var exception = new InvalidOperationException(
            "SQLite Error 19: 'FOREIGN KEY constraint failed'.");

        Assert.Equal(ExceptionCategory.ForeignKey, ExceptionFormatter.Categorize(exception));
        Assert.Equal(AppStrings.ErrorForeignKey, ExceptionFormatter.FriendlyMessage(exception));
    }

    [Fact]
    public void Unique_message_is_categorized_as_unique()
    {
        var exception = new InvalidOperationException(
            "UNIQUE constraint failed: Products.SKU");

        Assert.Equal(ExceptionCategory.UniqueConstraint, ExceptionFormatter.Categorize(exception));
        Assert.Equal(AppStrings.ErrorUnique, ExceptionFormatter.FriendlyMessage(exception));
    }

    [Fact]
    public void Not_null_message_is_categorized_as_not_null()
    {
        var exception = new InvalidOperationException(
            "NOT NULL constraint failed: SalesInvoices.CustomerId");

        Assert.Equal(ExceptionCategory.NotNull, ExceptionFormatter.Categorize(exception));
        Assert.Equal(AppStrings.ErrorNotNull, ExceptionFormatter.FriendlyMessage(exception));
    }

    [Fact]
    public void Concurrency_exception_is_categorized_as_concurrency()
    {
        var exception = new DbUpdateConcurrencyException("row changed");

        Assert.Equal(ExceptionCategory.Concurrency, ExceptionFormatter.Categorize(exception));
        Assert.Equal(AppStrings.ErrorConcurrency, ExceptionFormatter.FriendlyMessage(exception));
    }

    [Fact]
    public void Domain_exception_surfaces_its_own_message()
    {
        var exception = new DomainException("Insufficient stock");

        Assert.Equal(ExceptionCategory.Domain, ExceptionFormatter.Categorize(exception));
        Assert.Equal("Insufficient stock", ExceptionFormatter.FriendlyMessage(exception));
    }

    [Fact]
    public void Store_not_initialized_uses_the_friendly_localized_message()
    {
        var exception = new StoreNotInitializedException();

        Assert.Equal(ExceptionCategory.Domain, ExceptionFormatter.Categorize(exception));
        Assert.Equal(AppStrings.StoreNotInitialized, ExceptionFormatter.FriendlyMessage(exception));
    }

    [Fact]
    public void Unknown_exception_falls_back_to_the_generic_message()
    {
        var exception = new InvalidOperationException("boom");

        Assert.Equal(ExceptionCategory.Unknown, ExceptionFormatter.Categorize(exception));
        Assert.Equal(AppStrings.ErrorUnknown, ExceptionFormatter.FriendlyMessage(exception));
    }

    [Fact]
    public void Categorization_walks_the_inner_exception_chain()
    {
        var exception = new DbUpdateException(
            "save failed",
            new InvalidOperationException("SQLite Error 19: 'FOREIGN KEY constraint failed'."));

        Assert.Equal(ExceptionCategory.ForeignKey, ExceptionFormatter.Categorize(exception));
    }

    [Fact]
    public void Details_include_every_exception_in_the_chain()
    {
        var exception = new InvalidOperationException(
            "outer-marker",
            new InvalidOperationException("inner-marker"));

        var details = ExceptionFormatter.BuildDetails(exception);

        Assert.Contains("Category:", details);
        Assert.Contains("outer-marker", details);
        Assert.Contains("inner-marker", details);
    }
}
