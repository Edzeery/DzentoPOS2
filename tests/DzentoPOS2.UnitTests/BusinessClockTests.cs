using DzentoPOS2.App.Services.Systems;
using DzentoPOS2.Shared.Helpers;
using Xunit;

namespace DzentoPOS2.UnitTests;

public sealed class BusinessClockTests
{
    private static readonly DateTime FixedUtc =
        new(2026, 1, 1, 23, 30, 0, DateTimeKind.Utc);

    [Fact]
    public void TodayRangeFollowsConfiguredTimeZone()
    {
        var regional = new RegionalContext
        {
            TimeZoneId = "Asia/Tokyo"
        };

        var clock = new BusinessClock(regional, () => FixedUtc);

        Assert.Equal(new DateTime(2026, 1, 2), clock.Today);
        Assert.Equal(
            new DateTime(2026, 1, 1, 15, 0, 0, DateTimeKind.Utc),
            clock.TodayStartUtc);
        Assert.Equal(
            new DateTime(2026, 1, 2, 15, 0, 0, DateTimeKind.Utc),
            clock.TomorrowStartUtc);
    }

    [Fact]
    public void TodayRangeDiffersFromUtcDay()
    {
        var tokyo = new BusinessClock(
            new RegionalContext { TimeZoneId = "Asia/Tokyo" },
            () => FixedUtc);
        var utc = new BusinessClock(
            new RegionalContext { TimeZoneId = "UTC" },
            () => FixedUtc);

        Assert.NotEqual(utc.TodayStartUtc, tokyo.TodayStartUtc);
        Assert.NotEqual(utc.Today, tokyo.Today);
    }
}
