using DzentoPOS2.App.Services.Systems;
using DzentoPOS2.Shared.Contracts.DTOs.Reports;

namespace DzentoPOS2.App.Services.Reports
{
    public sealed record DashboardDateRange(
        DateTime StartUtc,
        DateTime EndUtc,
        DateTime PreviousStartUtc,
        DateTime PreviousEndUtc);

    public static class DashboardRangeResolver
    {
        public const int MaxCustomDays = 366;

        public static DashboardDateRange Resolve(IBusinessClock clock, DashboardFilterDto? filter)
        {
            if (clock is null) throw new ArgumentNullException(nameof(clock));
            filter ??= new DashboardFilterDto();

            var todayLocal = clock.Now.Date;

            DateTime startLocal;
            DateTime endLocalExclusive;

            switch (filter.Preset)
            {
                case DashboardDatePreset.Today:
                    startLocal = todayLocal;
                    endLocalExclusive = todayLocal.AddDays(1);
                    break;
                case DashboardDatePreset.Yesterday:
                    startLocal = todayLocal.AddDays(-1);
                    endLocalExclusive = todayLocal;
                    break;
                case DashboardDatePreset.Last7Days:
                    startLocal = todayLocal.AddDays(-6);
                    endLocalExclusive = todayLocal.AddDays(1);
                    break;
                case DashboardDatePreset.ThisMonth:
                    startLocal = new DateTime(todayLocal.Year, todayLocal.Month, 1);
                    endLocalExclusive = startLocal.AddMonths(1);
                    break;
                case DashboardDatePreset.Custom:
                    if (!filter.CustomFrom.HasValue || !filter.CustomTo.HasValue)
                        throw new ArgumentException("CustomFrom and CustomTo are required for Custom preset.");
                    var from = filter.CustomFrom.Value.Date;
                    var to = filter.CustomTo.Value.Date;
                    if (from > to)
                        throw new ArgumentException("CustomFrom must not be after CustomTo.");
                    if ((to - from).TotalDays + 1 > MaxCustomDays)
                        throw new ArgumentException($"Custom range must not exceed {MaxCustomDays} days.");
                    startLocal = from;
                    endLocalExclusive = to.AddDays(1);
                    break;
                default:
                    startLocal = todayLocal;
                    endLocalExclusive = todayLocal.AddDays(1);
                    break;
            }

            var startUtc = clock.StartOfLocalDayUtc(startLocal);
            var endUtc = clock.StartOfLocalDayUtc(endLocalExclusive);

            var span = endUtc - startUtc;
            var previousEndUtc = startUtc;
            var previousStartUtc = startUtc - span;

            return new DashboardDateRange(startUtc, endUtc, previousStartUtc, previousEndUtc);
        }
    }
}