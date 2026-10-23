using EZControls.Core.Enums;

namespace EZControls.Core.Base;

// ══════════════════════════════════════════════════════════════════════════════
//  EZSelectableBase
//
//  Improvements over the original:
//
//  1. CheckedChanged event:
//     The original had no event for when Checked changed.  Consumers typically
//     need to react to this (e.g. radio-group logic, form validation).
//
//  2. Toggle guard:
//     The original always toggled on click.  Controls that should only be
//     turned ON (e.g. a RadioButton) can override CanToggleOff.
//
//  3. Accessibility:
//     Sets AccessibleRole and AccessibleState so screen readers correctly
//     report the checked state.
// ══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Abstract base class for EZ controls that have a binary checked / unchecked state
/// (toggle buttons, checkboxes, radio buttons).
/// </summary>
public abstract class EZSelectableBase : EZControlBase
{
    private bool _checked;

    // ── Properties ────────────────────────────────────────────────────────────

    /// <summary>
    /// <see langword="true"/> when the control is in its checked / selected state.
    /// Setting this property raises <see cref="CheckedChanged"/> and calls
    /// <see cref="Control.Invalidate"/>.
    /// </summary>
    [Category("EZControls"), DefaultValue(false)]
    public bool Checked
    {
        get => _checked;
        set
        {
            if (_checked == value) return;
            _checked = value;
            OnCheckedChanged(EventArgs.Empty);
            Invalidate();
        }
    }

    /// <summary>
    /// When <see langword="false"/>, clicking a checked control does nothing.
    /// Override in <c>EZRadioButton</c> to prevent deselection.
    /// </summary>
    protected virtual bool CanToggleOff => true;

    // ── Events ────────────────────────────────────────────────────────────────

    /// <summary>Raised when <see cref="Checked"/> changes.</summary>
    [Category("EZControls")]
    public event EventHandler? CheckedChanged;

    /// <summary>Raises the <see cref="CheckedChanged"/> event.</summary>
    protected virtual void OnCheckedChanged(EventArgs e) =>
        CheckedChanged?.Invoke(this, e);

    // ── Input handling ────────────────────────────────────────────────────────

    /// <inheritdoc/>
    protected override void OnClick(EventArgs e)
    {
        base.OnClick(e);

        if (!_checked || CanToggleOff)
            Checked = !_checked;
    }

    // ── Accessibility ─────────────────────────────────────────────────────────

    /// <inheritdoc/>
    protected override AccessibleObject CreateAccessibilityInstance()
    {
        return new SelectableAccessibleObject(this);
    }

    private sealed class SelectableAccessibleObject : ControlAccessibleObject
    {
        private readonly EZSelectableBase _owner;

        public SelectableAccessibleObject(EZSelectableBase owner) : base(owner)
            => _owner = owner;

        public override AccessibleStates State =>
            base.State |
            (_owner.Checked ? AccessibleStates.Checked : AccessibleStates.None);
    }
}