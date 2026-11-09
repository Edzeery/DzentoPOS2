using DzentoPOS2.App.Services.Systems;
using Xunit;

namespace DzentoPOS2.UnitTests;

public class BranchSwitchGuardRegistryTests
{
    private sealed class FakeGuard : IBranchSwitchGuard
    {
        public bool CanSwitchBranch { get; set; } = true;
        public event EventHandler? CanSwitchChanged;
        public void RaiseChanged() => CanSwitchChanged?.Invoke(this, EventArgs.Empty);
    }

    [Fact]
    public void CanSwitch_ReturnsFalse_WhenAnyGuardReturnsFalse()
    {
        var registry = new BranchSwitchGuardRegistry();
        var guard1 = new FakeGuard { CanSwitchBranch = true };
        var guard2 = new FakeGuard { CanSwitchBranch = false };
        registry.Register(guard1);
        registry.Register(guard2);

        Assert.False(registry.CanSwitch);
    }

    [Fact]
    public void CanSwitch_ReturnsTrue_WhenAllGuardsAllow()
    {
        var registry = new BranchSwitchGuardRegistry();
        var guard1 = new FakeGuard { CanSwitchBranch = true };
        var guard2 = new FakeGuard { CanSwitchBranch = true };
        registry.Register(guard1);
        registry.Register(guard2);

        Assert.True(registry.CanSwitch);
    }
}
