using DzentoPOS2.App.Services.Reports;
using DzentoPOS2.App.Services.Systems;
using DzentoPOS2.Shared.Contracts.DTOs.Reports;
using DzentoPOS2.Shared.Helpers;
using Xunit;

namespace DzentoPOS2.UnitTests;

public sealed class DashboardRangeResolverTests
{
    private static (IBusinessClock clock, DateTime fixedUtc) MakeClock(string tzId, DateTime fixedUtc)
    {
        var regional = new RegionalContext { TimeZoneId = tzId };
        return (new BusinessClock(regional, () => fixedUtc), fixedUtc);
    }

    [Theory]
    [InlineData("Africa/Algiers")]
    [InlineData("Asia/Tokyo")]
    public void Today_produces_a_single_local_day_range(string tz)
    {
        var fixedUtc = new DateTime(2024, 6, 15, 10, 0, 0, DateTimeKind.Utc);
        var (clock, _) = MakeClock(tz, fixedUtc);

        var range = DashboardRangeResolver.Resolve(clock, new DashboardFilterDto { Preset = DashboardDatePreset.Today });

        Assert.Equal(1, (int)(range.EndUtc - range.StartUtc).TotalDays);
        // previous period has the same length
        Assert.Equal(range.EndUtc - range.StartUtc, range.PreviousEndUtc - range.PreviousStartUtc);
        Assert.Equal(range.StartUtc, range.PreviousEndUtc);
    }

    [Theory]
    [InlineData("Africa/Algiers")]
    [InlineData("Asia/Tokyo")]
    public void Yesterday_is_the_previous_local_day(string tz)
    {
        var fixedUtc = new DateTime(2024, 6, 15, 10, 0, 0, DateTimeKind.Utc);
        var (clock, _) = MakeClock(tz, fixedUtc);

        var today = DashboardRangeResolver.Resolve(clock, new DashboardFilterDto { Preset = DashboardDatePreset.Today });
        var yesterday = DashboardRangeResolver.Resolve(clock, new DashboardFilterDto { Preset = DashboardDatePreset.Yesterday });

        Assert.Equal(today.PreviousStartUtc, yesterday.StartUtc);
        Assert.Equal(today.PreviousEndUtc, yesterday.EndUtc);
    }

    [Theory]
    [InlineData("Africa/Algiers")]
    [InlineData("Asia/Tokyo")]
    public void Last7Days_spans_seven_local_days(string tz)
    {
        var fixedUtc = new DateTime(2024, 6, 15, 10, 0, 0, DateTimeKind.Utc);
        var (clock, _) = MakeClock(tz, fixedUtc);

        var range = DashboardRangeResolver.Resolve(clock, new DashboardFilterDto { Preset = DashboardDatePreset.Last7Days });

        Assert.Equal(7, (int)(range.EndUtc - range.StartUtc).TotalDays);
    }

    [Fact]
    public void ThisMonth_handles_leap_year_february()
    {
        var fixedUtc = new DateTime(2024, 2, 15, 10, 0, 0, DateTimeKind.Utc);
        var (clock, _) = MakeClock("Asia/Tokyo", fixedUtc);

        var range = DashboardRangeResolver.Resolve(clock, new DashboardFilterDto { Preset = DashboardDatePreset.ThisMonth });

        // February 2024 has 29 days
        Assert.Equal(29, (int)(range.EndUtc - range.StartUtc).TotalDays);
    }

    [Theory]
    [InlineData("Africa/Algiers")]
    [InlineData("Asia/Tokyo")]
    public void Custom_valid_range_resolves(string tz)
    {
        var fixedUtc = new DateTime(2024, 6, 15, 10, 0, 0, DateTimeKind.Utc);
        var (clock, _) = MakeClock(tz, fixedUtc);

        var from = fixedUtc.Date.AddDays(-2);
        var to = fixedUtc.Date;
        var range = DashboardRangeResolver.Resolve(clock, new DashboardFilterDto
        {
            Preset = DashboardDatePreset.Custom,
            CustomFrom = from,
            CustomTo = to
        });

        Assert.True(range.EndUtc > range.StartUtc);
        Assert.Equal(range.EndUtc - range.StartUtc, range.PreviousEndUtc - range.PreviousStartUtc);
    }

    [Fact]
    public void Custom_rejects_from_after_to()
    {
        var (clock, _) = MakeClock("Africa/Algiers", new DateTime(2024, 6, 15, 10, 0, 0, DateTimeKind.Utc));
        Assert.Throws<ArgumentException>(() => DashboardRangeResolver.Resolve(clock, new DashboardFilterDto
        {
            Preset = DashboardDatePreset.Custom,
            CustomFrom = new DateTime(2024, 6, 20),
            CustomTo = new DateTime(2024, 6, 10)
        }));
    }

    [Fact]
    public void Custom_rejects_over_366_days()
    {
        var (clock, _) = MakeClock("Africa/Algiers", new DateTime(2024, 6, 15, 10, 0, 0, DateTimeKind.Utc));
        Assert.Throws<ArgumentException>(() => DashboardRangeResolver.Resolve(clock, new DashboardFilterDto
        {
            Preset = DashboardDatePreset.Custom,
            CustomFrom = new DateTime(2023, 1, 1),
            CustomTo = new DateTime(2024, 12, 31)
        }));
    }

    [Fact]
    public void Custom_requires_both_dates()
    {
        var (clock, _) = MakeClock("Africa/Algiers", new DateTime(2024, 6, 15, 10, 0, 0, DateTimeKind.Utc));
        Assert.Throws<ArgumentException>(() => DashboardRangeResolver.Resolve(clock, new DashboardFilterDto
        {
            Preset = DashboardDatePreset.Custom,
            CustomFrom = new DateTime(2024, 6, 10)
        }));
    }

    [Fact]
    public void Midnight_boundary_algiers()
    {
        // just before midnight local vs just after
        var (clock, _) = MakeClock("Africa/Algiers", new DateTime(2024, 6, 15, 22, 59, 0, DateTimeKind.Utc));
        var r1 = DashboardRangeResolver.Resolve(clock, new DashboardFilterDto { Preset = DashboardDatePreset.Today });

        var (clock2, _) = MakeClock("Africa/Algiers", new DateTime(2024, 6, 15, 23, 1, 0, DateTimeKind.Utc));
        var r2 = DashboardRangeResolver.Resolve(clock2, new DashboardFilterDto { Preset = DashboardDatePreset.Today });

        // Algiers is UTC+1, so 22:59 UTC = 23:59 local (same day); 23:01 UTC = 00:01 next day
        Assert.NotEqual(r1.StartUtc, r2.StartUtc);
    }
}