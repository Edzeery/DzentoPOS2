using DzentoPOS2.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.App.Services.Sales;

/// <summary>
/// Store-level sales rules. Kept behind an interface so the permissions phase
/// can swap in a permission-aware implementation without touching
/// <see cref="SaleService"/>.
/// </summary>
public interface ISalesPolicy
{
    /// <summary>
    /// Maximum invoice discount (percent) a cashier may apply. Configurable
    /// through the app setting <c>sales.max_discount_percent</c> (default 100).
    ///
    /// Hook: a user holding the discount-override permission will be allowed to
    /// exceed this value in the permissions phase.
    /// </summary>
    Task<decimal> GetMaxDiscountPercentAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Reads <see cref="ISalesPolicy"/> values from the <c>AppSettings</c> table and
/// caches them for the lifetime of the scope.
/// </summary>
public sealed class SalesPolicy : ISalesPolicy
{
    public const string MaxDiscountPercentKey = "sales.max_discount_percent";

    public const decimal DefaultMaxDiscountPercent = 100m;

    private readonly AppDbContext _context;

    private decimal? _cachedMaxDiscountPercent;

    public SalesPolicy(AppDbContext context)
    {
        _context = context;
    }

    public async Task<decimal> GetMaxDiscountPercentAsync(
        CancellationToken cancellationToken = default)
    {
        if (_cachedMaxDiscountPercent is decimal cached)
            return cached;

        var raw = await _context.AppSettings
            .AsNoTracking()
            .Where(x => x.Key == MaxDiscountPercentKey)
            .Select(x => x.Value)
            .FirstOrDefaultAsync(cancellationToken);

        var value = DefaultMaxDiscountPercent;

        if (decimal.TryParse(
                raw,
                System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.InvariantCulture,
                out var parsed)
            && parsed >= 0
            && parsed <= 100)
        {
            value = parsed;
        }

        _cachedMaxDiscountPercent = value;
        return value;
    }
}