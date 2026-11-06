using DzentoPOS2.Shared.Helpers;

namespace DzentoPOS2.App.Services.Systems;

public sealed class BusinessClock : IBusinessClock
{
    private readonly RegionalContext _regional;
    private readonly Func<DateTime> _utcNow;

    public BusinessClock(
        RegionalContext regional,
        Func<DateTime>? utcNow = null)
    {
        _regional = regional;
        _utcNow = utcNow ?? (() => _regional.UtcNow);
    }

    public DateTime UtcNow => _utcNow();

    public DateTime Now => _regional.ToLocal(UtcNow);

    public DateTime Today => Now.Date;

    public DateTime TodayStartUtc => StartOfLocalDayUtc(Today);

    public DateTime TomorrowStartUtc => StartOfLocalDayUtc(Today.AddDays(1));

    public (DateTime StartUtc, DateTime EndUtc) TodayRangeUtc()
    {
        return (TodayStartUtc, TomorrowStartUtc);
    }

    public DateTime StartOfLocalDayUtc(DateTime localDay)
    {
        return _regional.StartOfLocalDayUtc(localDay);
    }

    public DateTime StartOfLocalMonthUtc()
    {
        return _regional.StartOfLocalMonthUtc();
    }

    public DateTime ToLocal(DateTime utc)
    {
        return _regional.ToLocal(utc);
    }

    public DateTime ToUtcSafe(DateTime local)
    {
        return _regional.ToUtcSafe(local);
    }
}
