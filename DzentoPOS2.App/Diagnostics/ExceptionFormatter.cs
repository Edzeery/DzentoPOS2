using System.Text;
using DzentoPOS2.Core.Exceptions;
using DzentoPOS2.Localization.Resources;
using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.App.Diagnostics;

/// <summary>
/// Turns any exception into a category, a friendly localized message and a full
/// technical detail block. The detail block walks the whole
/// <see cref="Exception.InnerException"/> chain so nothing is hidden.
/// </summary>
public static class ExceptionFormatter
{
    public static ExceptionCategory Categorize(Exception? exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            if (current is DbUpdateConcurrencyException)
                return ExceptionCategory.Concurrency;

            if (current is StoreNotInitializedException)
                return ExceptionCategory.Domain;

            if (current is DomainException)
                return ExceptionCategory.Domain;

            var message = current.Message ?? string.Empty;

            if (message.Contains("FOREIGN KEY", StringComparison.OrdinalIgnoreCase))
                return ExceptionCategory.ForeignKey;

            if (message.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase))
                return ExceptionCategory.UniqueConstraint;

            if (message.Contains("NOT NULL", StringComparison.OrdinalIgnoreCase))
                return ExceptionCategory.NotNull;
        }

        return ExceptionCategory.Unknown;
    }

    /// <summary>
    /// Localized, user-friendly message for the exception's category.
    /// </summary>
    public static string FriendlyMessage(Exception exception)
    {
        return Categorize(exception) switch
        {
            ExceptionCategory.ForeignKey => AppStrings.ErrorForeignKey,
            ExceptionCategory.UniqueConstraint => AppStrings.ErrorUnique,
            ExceptionCategory.NotNull => AppStrings.ErrorNotNull,
            ExceptionCategory.Concurrency => AppStrings.ErrorConcurrency,
            ExceptionCategory.Domain => DomainMessage(exception),
            _ => AppStrings.ErrorUnknown
        };
    }

    /// <summary>
    /// Full technical detail: category, every exception in the chain and the
    /// tracked entities of a failed <see cref="DbUpdateException"/>.
    /// </summary>
    public static string BuildDetails(Exception? exception)
    {
        var builder = new StringBuilder();

        if (exception is null)
            return string.Empty;

        builder.AppendLine($"Category: {Categorize(exception)}");
        builder.AppendLine($"Occurred: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        builder.AppendLine();

        var depth = 0;
        for (var current = exception; current is not null; current = current.InnerException)
        {
            builder.AppendLine($"[{depth}] {current.GetType().FullName}: {current.Message}");

            if (current is DbUpdateException update && update.Entries.Count > 0)
            {
                builder.AppendLine("  Entities:");
                foreach (var entry in update.Entries)
                    builder.AppendLine($"    {entry.Metadata.ClrType.Name} ({entry.State})");
            }

            if (!string.IsNullOrWhiteSpace(current.StackTrace))
            {
                builder.AppendLine("  StackTrace:");
                builder.AppendLine(current.StackTrace);
            }

            builder.AppendLine();
            depth++;
        }

        return builder.ToString();
    }

    private static string DomainMessage(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            if (current is StoreNotInitializedException)
                return AppStrings.StoreNotInitialized;

            if (current is CashSessionRequiredException)
                return AppStrings.NoOpenSession;

            if (current is CodedDomainException coded)
            {
                var localized = LocalizeCode(coded.Code);
                if (localized is not null)
                    return localized;
            }
        }

        // Domain messages are written for the user, so surface them directly.
        return exception is DomainException
            ? exception.Message
            : AppStrings.ErrorDomain;
    }

    /// <summary>
    /// Maps a stable <see cref="DomainErrorCodes"/> entry to a localized
    /// message. Returns <c>null</c> for codes without a dedicated string so
    /// the caller can fall back to the raw (developer) message.
    /// </summary>
    private static string? LocalizeCode(string code) => code switch
    {
        DomainErrorCodes.DiscountExceedsSubtotal => AppStrings.DiscountExceedsSubtotal,
        DomainErrorCodes.DiscountExceedsLimit => AppStrings.DiscountExceedsLimit,
        DomainErrorCodes.InvalidTaxRate => AppStrings.InvalidTaxRate,
        DomainErrorCodes.CreditRequiresCustomer => AppStrings.CreditSaleRequiresCustomer,
        DomainErrorCodes.InsufficientStock => AppStrings.InsufficientStock,
        DomainErrorCodes.NoOpenSession => AppStrings.NoOpenSession,
        DomainErrorCodes.SaleRequiresItem => AppStrings.EmptyCart,
        DomainErrorCodes.SessionAlreadyOpen => AppStrings.SessionAlreadyOpen,
        DomainErrorCodes.SessionAlreadyClosed => AppStrings.SessionAlreadyClosed,
        DomainErrorCodes.SessionNotFound => AppStrings.NoOpenSession,
        DomainErrorCodes.CashboxNotFound => AppStrings.ErrorDomain,
        _ => null
    };
}