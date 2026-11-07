using System;

namespace DzentoPOS2.App.Services.Inventory
{
    public sealed class InventoryChangedNotifier : IInventoryChangedNotifier
    {
        public event EventHandler? Changed;

        public void NotifyChanged() => Changed?.Invoke(this, EventArgs.Empty);
    }
}