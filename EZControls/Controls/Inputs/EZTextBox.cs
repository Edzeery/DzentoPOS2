using System.ComponentModel;
using EZControls.Core.Base;

namespace EZControls.Controls.Inputs;

// ══════════════════════════════════════════════════════════════════════════════
//  EZTextBox
//
//  EZTextBox is intentionally minimal: EZInputBase supplies every feature
//  (placeholder, border, validation, theming, DPI).  This class exists so that:
//    • The control has a distinct ToolboxItem type name.
//    • Subclasses of EZInputBase can be created for specialised inputs
//      (EZNumericBox, EZPasswordBox, etc.) without changing the base class.
//    • Designers show a focused, named node in the component tree.
// ══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// A styled single-line or multi-line text input.
/// All functionality is provided by <see cref="EZInputBase"/>.
/// </summary>
[DesignerCategory("Code")]
[DefaultEvent(nameof(TextChanged))]
[ToolboxBitmap(typeof(TextBox))]
public class EZTextBox : EZInputBase
{
    public EZTextBox()
    {
        if (InnerTextBox is not null)
            InnerTextBox.KeyDown += (_, e) => OnKeyDown(e);
    }
}
