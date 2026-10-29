using DzentoPOS2.App.Services.Accounting;
using Xunit;

namespace DzentoPOS2.UnitTests;

/// <summary>
/// The drawer belongs to a local business day: a session opened at 23:59
/// yesterday must be flagged as stale at 00:01 today, while a session opened
/// at 00:01 today must not be.
/// </summary>
public sealed class CashSessionPolicyTests
{
    [Fact]
    public void Session_opened_on_a_previous_local_day_is_flagged()
    {
        var today = new DateTime(2026, 10, 27, 0, 1, 0);
        var openedYesterday = new DateTime(2026, 10, 26, 23, 59, 0);

        Assert.True(CashSessionPolicy.IsFromPreviousBusinessDay(openedYesterday, today));
    }

    [Fact]
    public void Session_opened_earlier_the_same_local_day_is_not_flagged()
    {
        var today = new DateTime(2026, 10, 27, 18, 30, 0);
        var openedToday = new DateTime(2026, 10, 27, 0, 1, 0);

        Assert.False(CashSessionPolicy.IsFromPreviousBusinessDay(openedToday, today));
    }

    [Fact]
    public void Session_opened_at_exactly_midnight_today_is_not_flagged()
    {
        var today = new DateTime(2026, 10, 27, 9, 0, 0);
        var openedMidnight = new DateTime(2026, 10, 27, 0, 0, 0);

        Assert.False(CashSessionPolicy.IsFromPreviousBusinessDay(openedMidnight, today));
    }
}
