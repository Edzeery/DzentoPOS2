namespace DzentoPOS2.App.Services.Accounting;

/// <summary>
/// Pure cash-drawer rules shared by the shell and the tests. The business day
/// is always the local day (the caller converts the UTC timestamp with
/// <c>IBusinessClock.ToLocal</c>), never the UTC date.
/// </summary>
public static class CashSessionPolicy
{
    /// <summary>
    /// True when a session opened on <paramref name="openedAtLocal"/> belongs to
    /// an earlier local business day than <paramref name="todayLocal"/>.
    /// Comparing dates (not instants) means a session opened at 23:59 yesterday
    /// is stale at 00:01 today, while one opened at 00:01 today is not.
    /// </summary>
    public static bool IsFromPreviousBusinessDay(DateTime openedAtLocal, DateTime todayLocal)
        => openedAtLocal.Date < todayLocal.Date;
}