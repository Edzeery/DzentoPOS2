using System.ComponentModel;
using System.Globalization;
using EZControls.Core.Base;
using EZControls.Core.Enums;

namespace EZControls.Controls.Inputs;

// ══════════════════════════════════════════════════════════════════════════════
//  EZNumericBox
//
//  A numeric input built on EZInputBase.  It deliberately does NOT wrap a
//  NumericUpDown:
//    • NumericUpDown cannot be re-themed (native black border + spin buttons),
//      which is exactly what the POS fields had to get rid of.
//    • EZInputBase already paints the rounded border, focus ring, placeholder,
//      validation state and dark/RTL support, so subclassing it keeps every
//      numeric field pixel-identical to the text fields.
//
//  Number rendering and parsing use one explicit CultureInfo.  The POS sets it
//  to the same normalised culture used by RegionalContext.FormatMoney, so the
//  hero total and the editable fields always show the same decimal separator
//  and Western digits.
// ══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// A styled numeric input with <see cref="Minimum"/>, <see cref="Maximum"/>,
/// <see cref="DecimalPlaces"/> and a committed <see cref="Value"/>.
/// </summary>
[DesignerCategory("Code")]
[DefaultEvent(nameof(ValueChanged))]
[ToolboxBitmap(typeof(NumericUpDown))]
public class EZNumericBox : EZInputBase
{
    private const int DefaultHeight = 40;

    private decimal _value;
    private decimal _minimum;
    private decimal _maximum = 999_999_999m;
    private decimal _increment = 1m;
    private int _decimalPlaces = 2;
    private decimal _lastCommitted;
    private CultureInfo _culture = CultureInfo.CurrentCulture;
    private bool _formatting;

    /// <summary>Initialises a right-aligned numeric input.</summary>
    public EZNumericBox()
    {
        Size = new Size(170, DefaultHeight);
        TextAlign = HorizontalAlignment.Right;
        PlaceholderText = string.Empty;

        if (InnerTextBox is not null)
        {
            // Select the whole number as soon as the field is entered so typing
            // replaces the old value instead of appending to it.
            InnerTextBox.Enter += (_, _) =>
            {
                if (IsInDesigner)
                    return;

                InnerTextBox.BeginInvoke(new Action(() =>
                {
                    if (InnerTextBox is { IsDisposed: false, Focused: true })
                        InnerTextBox.SelectAll();
                }));
            };

            InnerTextBox.KeyDown += OnInnerKeyDown;
            InnerTextBox.Leave += (_, _) => Commit();
            InnerTextBox.MouseWheel += (_, e) =>
            {
                if (e is HandledMouseEventArgs handled)
                    handled.Handled = true;
            };
        }

        _lastCommitted = _value;
        FormatValue();
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Events
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>Raised when the committed <see cref="Value"/> changes.</summary>
    [Category("EZControls")]
    public event EventHandler? ValueChanged;

    // ══════════════════════════════════════════════════════════════════════════
    //  Value API
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>Committed value, always clamped and rounded to <see cref="DecimalPlaces"/>.</summary>
    [Category("EZControls")]
    [DefaultValue(typeof(decimal), "0")]
    public decimal Value
    {
        get => _value;
        set => SetCommittedValue(value, raiseEvent: true);
    }

    /// <summary>Smallest accepted value. Defaults to zero.</summary>
    [Category("EZControls")]
    [DefaultValue(typeof(decimal), "0")]
    public decimal Minimum
    {
        get => _minimum;
        set
        {
            _minimum = value;
            if (_maximum < _minimum)
                _maximum = _minimum;
            SetCommittedValue(_value, raiseEvent: true);
        }
    }

    /// <summary>Largest accepted value.</summary>
    [Category("EZControls")]
    public decimal Maximum
    {
        get => _maximum;
        set
        {
            _maximum = value;
            if (_minimum > _maximum)
                _minimum = _maximum;
            SetCommittedValue(_value, raiseEvent: true);
        }
    }

    /// <summary>Number of decimals kept after the separator (0-6).</summary>
    [Category("EZControls")]
    [DefaultValue(2)]
    public int DecimalPlaces
    {
        get => _decimalPlaces;
        set
        {
            _decimalPlaces = Math.Clamp(value, 0, 6);
            SetCommittedValue(_value, raiseEvent: false);
        }
    }

    /// <summary>Amount added/subtracted by the Up/Down keys.</summary>
    [Category("EZControls")]
    [DefaultValue(typeof(decimal), "1")]
    public decimal Increment
    {
        get => _increment;
        set => _increment = value == 0m ? 1m : Math.Abs(value);
    }

    /// <summary>
    /// Culture used to format and parse the number.  Set it to the same
    /// normalised culture as the money labels (Western digits, one separator).
    /// </summary>
    [Category("EZControls")]
    [Browsable(false)]
    public CultureInfo Culture
    {
        get => _culture;
        set
        {
            _culture = value ?? CultureInfo.CurrentCulture;
            FormatValue();
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Public helpers
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>Selects the whole number so the next keystroke replaces it.</summary>
    public void SelectAllText() => InnerTextBox?.SelectAll();

    /// <summary>
    /// Applies the accessible name and description to both the control and its
    /// inner editor (which is the control that actually receives focus).
    /// </summary>
    public void SetAccessibleText(string name, string description)
    {
        AccessibleName = name;
        AccessibleDescription = description;

        if (InnerTextBox is not null)
        {
            InnerTextBox.AccessibleName = name;
            InnerTextBox.AccessibleDescription = description;
        }
    }

    /// <summary>Commits the typed text to <see cref="Value"/>.</summary>
    public void Commit()
    {
        if (_formatting || InnerTextBox is null)
            return;

        string raw = InnerTextBox.Text;
        if (string.IsNullOrWhiteSpace(raw))
        {
            SetCommittedValue(_minimum > 0m ? _minimum : 0m, raiseEvent: true);
            return;
        }

        if (!TryParse(raw, out decimal parsed))
        {
            // An unparsable text is discarded, not stored.
            FormatValue();
            return;
        }

        SetCommittedValue(parsed, raiseEvent: true);
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Layout
    // ══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    protected override void OnCreateControl()
    {
        base.OnCreateControl();
        Height = DefaultHeight;

        if (InnerTextBox is not null)
        {
            InnerTextBox.AccessibleName = AccessibleName;
            InnerTextBox.AccessibleDescription = AccessibleDescription;
        }
    }

    /// <inheritdoc/>
    protected override void OnFontChanged(EventArgs e)
    {
        base.OnFontChanged(e);
        // EZInputBase derives the height from the font; the POS grid reserves a
        // fixed 40 px for every field, so the height must not drift.
        Height = DefaultHeight;
    }

    /// <summary>Mouse wheel never changes the numeric value.</summary>
    protected override void OnMouseWheel(MouseEventArgs e)
    {
        if (e is HandledMouseEventArgs handled)
            handled.Handled = true;
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing && InnerTextBox is not null)
        {
            InnerTextBox.KeyDown -= OnInnerKeyDown;
        }

        base.Dispose(disposing);
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Internals
    // ══════════════════════════════════════════════════════════════════════════

    private void OnInnerKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode is Keys.Enter)
        {
            Commit();
            SelectAllText();
            e.SuppressKeyPress = true;
            e.Handled = true;
            return;
        }

        if (e.KeyCode is Keys.Escape)
        {
            // Escape restores the last committed value.
            _value = _lastCommitted;
            FormatValue();
            SelectAllText();
            e.SuppressKeyPress = true;
            e.Handled = true;
            return;
        }

        if (e.KeyCode is Keys.Up or Keys.Down)
        {
            decimal next = e.KeyCode == Keys.Up ? _value + _increment : _value - _increment;
            SetCommittedValue(next, raiseEvent: true);
            SelectAllText();
            e.SuppressKeyPress = true;
            e.Handled = true;
        }
    }

    private void SetCommittedValue(decimal value, bool raiseEvent)
    {
        decimal rounded = Math.Round(value, _decimalPlaces, MidpointRounding.AwayFromZero);
        decimal clamped = Math.Clamp(rounded, _minimum, _maximum);
        bool changed = clamped != _value;

        _value = clamped;
        _lastCommitted = clamped;
        FormatValue();

        if (changed && raiseEvent)
            ValueChanged?.Invoke(this, EventArgs.Empty);
    }

    private void FormatValue()
    {
        if (InnerTextBox is null)
            return;

        _formatting = true;
        try
        {
            NumberFormatInfo format = BuildFormatInfo(_culture);
            InnerTextBox.Text = _value.ToString("N" + _decimalPlaces, format);
            InnerTextBox.Select(InnerTextBox.TextLength, 0);
        }
        finally
        {
            _formatting = false;
        }
    }

    private bool TryParse(string text, out decimal parsed)
    {
        parsed = 0m;

        string normalized = NormalizeDigits(text).Trim().Replace("\u00A0", string.Empty).Replace(" ", string.Empty);
        if (normalized.Length == 0)
            return false;

        NumberFormatInfo format = BuildFormatInfo(_culture);
        string decimalSeparator = format.NumberDecimalSeparator;
        string altSeparator = decimalSeparator == "." ? "," : ".";

        normalized = normalized.Replace("\u066C", string.Empty).Replace("\u066B", decimalSeparator);

        int decimalIndex = normalized.LastIndexOf(decimalSeparator, StringComparison.Ordinal);
        int altIndex = normalized.LastIndexOf(altSeparator, StringComparison.Ordinal);

        if (decimalIndex >= 0 && altIndex >= 0)
        {
            // Both separators are present: the right-most one is the decimal
            // separator, the other is grouping.
            if (decimalIndex > altIndex)
                normalized = normalized.Replace(altSeparator, string.Empty);
            else
                normalized = normalized.Replace(decimalSeparator, string.Empty);
        }
        else if (altIndex >= 0 && decimalIndex < 0)
        {
            int digitsAfter = normalized.Length - altIndex - 1;
            bool looksDecimal = _decimalPlaces > 0
                && digitsAfter > 0
                && digitsAfter <= _decimalPlaces;

            normalized = looksDecimal
                ? normalized.Remove(altIndex, 1).Insert(altIndex, decimalSeparator)
                : normalized.Replace(altSeparator, string.Empty);
        }

        return decimal.TryParse(
            normalized,
            NumberStyles.Number | NumberStyles.AllowExponent,
            format,
            out parsed);
    }

    private static NumberFormatInfo BuildFormatInfo(CultureInfo culture)
    {
        var format = (NumberFormatInfo)culture.NumberFormat.Clone();
        format.NativeDigits = new[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
        format.DigitSubstitution = DigitShapes.None;
        return format;
    }

    private static string NormalizeDigits(string text)
    {
        Span<char> buffer = text.Length <= 256 ? stackalloc char[text.Length] : new char[text.Length];

        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            buffer[i] = c switch
            {
                >= '\u0660' and <= '\u0669' => (char)('0' + (c - '\u0660')),
                >= '\u06F0' and <= '\u06F9' => (char)('0' + (c - '\u06F0')),
                _ => c
            };
        }

        return new string(buffer);
    }
}
