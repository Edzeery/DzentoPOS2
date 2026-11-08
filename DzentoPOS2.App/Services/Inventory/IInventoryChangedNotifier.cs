namespace DzentoPOS2.App.Services.Inventory
{
    /// <summary>
    /// Tiny invalidation channel for inventory-derived caches (valuation, dashboard).
    /// Services that mutate stock/prices call <see cref="NotifyChanged"/> after commit.
    /// </summary>
    public interface IInventoryChangedNotifier
    {
        event EventHandler? Changed;

        void NotifyChanged();
    }
}