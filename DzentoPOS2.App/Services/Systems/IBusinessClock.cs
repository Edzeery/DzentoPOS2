namespace DzentoPOS2.App.Services.Systems;

public interface IBusinessClock
{
    DateTime UtcNow { get; }

    DateTime Now { get; }

    DateTime Today { get; }

    DateTime TodayStartUtc { get; }

    DateTime TomorrowStartUtc { get; }

    (DateTime StartUtc, DateTime EndUtc) TodayRangeUtc();

    DateTime StartOfLocalDayUtc(DateTime localDay);

    DateTime StartOfLocalMonthUtc();

    DateTime ToLocal(DateTime utc);

    DateTime ToUtcSafe(DateTime local);
}
