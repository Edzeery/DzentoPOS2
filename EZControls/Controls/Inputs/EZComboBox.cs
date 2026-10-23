using EZControls.Core.Base;
using EZControls.Core.Constants;
using EZControls.Core.Drawing;
using EZControls.Core.Enums;
using EZControls.Core.Themes;
using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace EZControls.Controls.Inputs;

public enum EZDropDownPlacement
{
    Bottom,
    Top,
    Left,
    Right
}

/// <summary>
/// A fully-custom styled combo-box control with improved reliability and features.
/// Wraps a native ComboBox for data and draws its own visual chrome.
/// Supports standard Add() API and improved dropdown styling.
/// </summary>
[DefaultEvent(nameof(SelectedIndexChanged))]
[ToolboxBitmap(typeof(ComboBox))]
[DesignerCategory("Code")]
public class EZComboBox : EZControlBase
{
    // ── Inner native control ──────────────────────────────────────────────────
    private ComboBox? _inner;

    // ── Visual fields ─────────────────────────────────────────────────────────
    private Color _borderColor = EZDefaultColors.Border;
    private Color _borderFocusColor = EZDefaultColors.PrimaryFocus;
    private Color _iconColor = EZDefaultColors.Text;
    private Color _dropDownBackColor = Color.White;
    private Color _dropDownForeColor = EZDefaultColors.Text;
    private Color _dropDownBorderColor = EZDefaultColors.Border;
    private Color _dropDownHoverBackColor = Color.FromArgb(245, 248, 250);
    private Color _dropDownSelectedBackColor = Color.FromArgb(229, 246, 245);
    private Color _dropDownSelectedForeColor = EZDefaultColors.Text;
    private int _borderSize = EZDefaults.DefaultBorderSize;
    private int _borderRadius = EZDefaults.DefaultBorderRadius;
    private bool _isDroppedDown;
    private ToolStripDropDown? _dropDown;

    // ── Items storage ─────────────────────────────────────────────────────────
    private string[] _items = Array.Empty<string>();

    // ── Dropdown styling ───────────────────────────────────────────────────
    private int _dropDownHeight = 200;
    private int _dropDownItemHeight = 20;
    private int _maxDropDownItems = 0;  // 0 = use DropDownHeight
    private int _dropDownBorderRadius = 10;
    private int _dropDownGap = 7;
    private EZDropDownPlacement _dropDownPlacement = EZDropDownPlacement.Bottom;

    // ── Auto-complete ─────────────────────────────────────────────────────
    private AutoCompleteMode _autoCompleteMode = AutoCompleteMode.None;
    private AutoCompleteSource _autoCompleteSource = AutoCompleteSource.ListItems;

    // ── Accessibility ────────────────────────────────────────────────────
    private string _accessibleName = string.Empty;
    private string _accessibleDescription = string.Empty;

    // ══════════════════════════════════════════════════════════════════════════
    //  Constructor
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>Initialises the combo box with default visual settings.</summary>
    public EZComboBox()
    {
        SuspendLayout();

        try
        {
            // ── Default appearance first (before creating inner control) ────
            Size = new Size(250, 38);
            MinimumSize = new Size(100, 28);

            BackColor = Color.White;
            ForeColor = EZDefaultColors.Text;

            Font = SystemFonts.DefaultFont;
            Cursor = Cursors.Hand;

            // ── Native ComboBox ────────────────────────────────────────────────
            _inner = new ComboBox
            {
                DropDownHeight = _dropDownHeight,
                ItemHeight = _dropDownItemHeight,
                FormattingEnabled = true,
                IntegralHeight = false,
                FlatStyle = FlatStyle.Flat,
                Font = Font,
                Visible = false,
                TabStop = false,
                BackColor = _dropDownBackColor,
                ForeColor = _dropDownForeColor,
                AutoCompleteMode = _autoCompleteMode,
                AutoCompleteSource = _autoCompleteSource
            };

            Controls.Add(_inner);

            // ── Wire Events ────────────────────────────────────────────────────
            _inner.SelectedIndexChanged += (_, e) =>
            {
                SelectedIndexChanged?.Invoke(this, e);
                Invalidate();
            };

            _inner.TextChanged += (_, _) => Invalidate();

            _inner.DropDown += (_, _) =>
            {
                if (!IsInDesigner)
                {
                    _isDroppedDown = true;
                    Invalidate();
                }
            };

            _inner.DropDownClosed += (_, _) =>
            {
                if (!IsInDesigner)
                {
                    _isDroppedDown = false;
                    State = EZControlState.Normal;
                    Invalidate();

                    // Restore focus to wrapper, not inner control
                    if (!Focused)
                        Focus();
                }
            };

            _inner.GotFocus += (_, _) =>
            {
                // Don't let inner control handle focus; keep it on wrapper
                if (!IsInDesigner && !_isDroppedDown)
                    this.Focus();
            };

            _inner.LostFocus += (_, _) =>
            {
                if (!IsInDesigner && !_isDroppedDown)
                {
                    State = EZControlState.Normal;
                    Invalidate();
                }
            };

            // Sync items array to inner control
            SyncItemsToInner();
        }
        finally
        {
            ResumeLayout(false);
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Events
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>Raised when the selected item in the drop-down list changes.</summary>
    [Category("EZControls")]
    public event EventHandler? SelectedIndexChanged;

    // ══════════════════════════════════════════════════════════════════════════
    //  Data / ComboBox passthrough properties
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>Drop-down list style (DropDown or DropDownList).</summary>
    [Category("EZControls"), DefaultValue(ComboBoxStyle.DropDown)]
    public ComboBoxStyle DropDownStyle
    {
        get => _inner?.DropDownStyle ?? ComboBoxStyle.DropDown;
        set
        {
            if (_inner is not null)
                _inner.DropDownStyle = value;
        }
    }

    /// <summary>Data source for the combo box items.</summary>
    [Category("EZControls"), DefaultValue(null)]
    public object? DataSource
    {
        get => _inner?.DataSource;
        set
        {
            if (_inner is not null)
                _inner.DataSource = value;
        }
    }

    /// <summary>Property of the data source to display.</summary>
    [Category("EZControls"), DefaultValue("")]
    public string DisplayMember
    {
        get => _inner?.DisplayMember ?? string.Empty;
        set
        {
            if (_inner is not null)
                _inner.DisplayMember = value;
        }
    }

    /// <summary>Property of the data source to use as the value.</summary>
    [Category("EZControls"), DefaultValue("")]
    public string ValueMember
    {
        get => _inner?.ValueMember ?? string.Empty;
        set
        {
            if (_inner is not null)
                _inner.ValueMember = value;
        }
    }

    /// <summary>The currently selected item.</summary>
    [Category("EZControls"), Browsable(false)]
    public object? SelectedItem
    {
        get => _inner?.SelectedItem;
        set
        {
            if (_inner is not null)
                _inner.SelectedItem = value;
        }
    }

    /// <summary>Zero-based index of the currently selected item, or -1 if none.</summary>
    [Category("EZControls"), Browsable(false)]
    public int SelectedIndex
    {
        get => _inner?.SelectedIndex ?? -1;
        set
        {
            if (_inner is not null)
            {
                if (value < -1 || value >= _inner.Items.Count)
                    throw new ArgumentOutOfRangeException(nameof(SelectedIndex),
                        $"Index must be between -1 and {_inner.Items.Count - 1}");

                _inner.SelectedIndex = value;
            }
        }
    }

    /// <summary>The value of the selected item's <see cref="ValueMember"/> property.</summary>
    [Browsable(false)]
    public object? SelectedValue
    {
        get => _inner?.SelectedValue;
        set
        {
            if (_inner is not null)
                _inner.SelectedValue = value;
        }
    }

    /// <summary>
    /// Items as text (one item per line). Edit directly in the designer.
    /// Much simpler than the array editor - just type your items!
    /// </summary>
    [Category("EZControls")]
    [DefaultValue("")]
    [Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design", "System.Drawing.Design.UITypeEditor")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public string ItemsText
    {
        get => string.Join(Environment.NewLine, _items);
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                _items = Array.Empty<string>();
            }
            else
            {
                _items = value
                    .Split(new[] { Environment.NewLine, "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(item => item.Trim())
                    .Where(item => !string.IsNullOrEmpty(item))
                    .ToArray();
            }
            SyncItemsToInner();
        }
    }

    /// <summary>Items as array (for code access). Use ItemsText in designer.</summary>
    [Browsable(false)]
    public string[] Items
    {
        get => _items;
        set
        {
            _items = value ?? Array.Empty<string>();
            SyncItemsToInner();
        }
    }

    /// <summary>The text of the currently selected item, or empty string.</summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string SelectedText
    {
        get => SelectedIndex >= 0 && SelectedIndex < _items.Length
            ? _items[SelectedIndex]
            : string.Empty;
    }

    /// <summary>The display text of the currently selected item.</summary>
    [Category("EZControls"), Browsable(true)]
    [System.Diagnostics.CodeAnalysis.AllowNull]
    public override string Text
    {
        get => _inner?.Text ?? string.Empty;
        set
        {
            if (_inner is not null)
                _inner.Text = value ?? string.Empty;
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Item Management Methods (Standard ComboBox API)
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Add a single item to the combo box (standard ComboBox API).
    /// Usage: combo.Add("Egypt");
    /// </summary>
    public void Add(string item)
    {
        if (string.IsNullOrWhiteSpace(item))
            return;

        var list = _items.ToList();
        list.Add(item.Trim());
        Items = list.ToArray();
    }

    /// <summary>
    /// Add multiple items at once (standard API).
    /// Usage: combo.Add("Item1", "Item2", "Item3");
    /// </summary>
    public void Add(params string[] items)
    {
        if (items == null || items.Length == 0)
            return;

        var list = _items.ToList();
        foreach (var item in items.Where(i => !string.IsNullOrWhiteSpace(i)))
            list.Add(item.Trim());

        Items = list.ToArray();
    }

    /// <summary>Add a single item to the combo box.</summary>
    public void AddItem(string item) => Add(item);

    /// <summary>Add multiple items at once.</summary>
    public void AddItems(params string[] items) => Add(items);

    /// <summary>Remove an item by value.</summary>
    public void RemoveItem(string item)
    {
        if (string.IsNullOrWhiteSpace(item))
            return;

        var list = _items.ToList();
        list.Remove(item.Trim());
        Items = list.ToArray();
    }

    /// <summary>Remove an item by index.</summary>
    public void RemoveItemAt(int index)
    {
        if (index < 0 || index >= _items.Length)
            throw new ArgumentOutOfRangeException(nameof(index),
                $"Index must be between 0 and {_items.Length - 1}");

        var list = _items.ToList();
        list.RemoveAt(index);
        Items = list.ToArray();
    }

    /// <summary>Clear all items.</summary>
    public void ClearItems() => Items = Array.Empty<string>();

    /// <summary>Check if an item exists.</summary>
    public bool Contains(string item)
    {
        return !string.IsNullOrWhiteSpace(item) && _items.Contains(item.Trim());
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Visual properties
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>Border color in the idle state.</summary>
    [Category("EZControls")]
    public Color BorderColor
    {
        get => _borderColor;
        set { _borderColor = value; Invalidate(); }
    }

    /// <summary>Border color when the control holds focus or is open.</summary>
    [Category("EZControls")]
    public Color BorderFocusColor
    {
        get => _borderFocusColor;
        set { _borderFocusColor = value; Invalidate(); }
    }

    /// <summary>Color of the chevron icon.</summary>
    [Category("EZControls")]
    public Color IconColor
    {
        get => _iconColor;
        set { _iconColor = value; Invalidate(); }
    }

    /// <summary>Border pen width in logical pixels.</summary>
    [Category("EZControls"), DefaultValue(1)]
    public int BorderSize
    {
        get => _borderSize;
        set { _borderSize = Math.Max(1, value); Invalidate(); }
    }

    /// <summary>Corner arc radius in logical pixels.</summary>
    [Category("EZControls"), DefaultValue(8)]
    public int BorderRadius
    {
        get => _borderRadius;
        set { _borderRadius = Math.Max(0, value); Invalidate(); }
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Dropdown styling properties
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>Height of the drop-down list in pixels.</summary>
    [Category("EZControls"), DefaultValue(200)]
    public int DropDownHeight
    {
        get => _dropDownHeight;
        set
        {
            _dropDownHeight = Math.Max(50, value);
            if (_inner is not null)
                _inner.DropDownHeight = _dropDownHeight;
        }
    }

    /// <summary>Height of each item in the drop-down list in pixels (controls font size).</summary>
    [Category("EZControls"), DefaultValue(20)]
    public int ItemHeight
    {
        get => _dropDownItemHeight;
        set
        {
            _dropDownItemHeight = Math.Max(12, value);
            if (_inner is not null)
                _inner.ItemHeight = _dropDownItemHeight;
        }
    }

    /// <summary>
    /// Maximum number of items to display before scrolling (0 = use DropDownHeight).
    /// If set, automatically calculates optimal DropDownHeight.
    /// </summary>
    [Category("EZControls"), DefaultValue(0)]
    public int MaxDropDownItems
    {
        get => _maxDropDownItems;
        set
        {
            _maxDropDownItems = Math.Max(0, value);
            if (_maxDropDownItems > 0)
                DropDownHeight = (_maxDropDownItems * ItemHeight) + 4;
        }
    }

    /// <summary>Background color of the dropdown list (makes it consistent with control).</summary>
    [Category("EZControls")]
    public Color DropDownBackColor
    {
        get => _dropDownBackColor;
        set
        {
            _dropDownBackColor = value;
            if (_inner is not null)
                _inner.BackColor = value;
            InvalidateDropDown();
        }
    }

    /// <summary>Text color of the dropdown items (makes it consistent with control).</summary>
    [Category("EZControls")]
    public Color DropDownForeColor
    {
        get => _dropDownForeColor;
        set
        {
            _dropDownForeColor = value;
            if (_inner is not null)
                _inner.ForeColor = value;
            InvalidateDropDown();
        }
    }

    /// <summary>Border color used by the custom drop-down popup.</summary>
    [Category("EZControls")]
    public Color DropDownBorderColor
    {
        get => _dropDownBorderColor;
        set { _dropDownBorderColor = value; InvalidateDropDown(); }
    }

    /// <summary>Background color used when hovering a drop-down item.</summary>
    [Category("EZControls")]
    public Color DropDownHoverBackColor
    {
        get => _dropDownHoverBackColor;
        set { _dropDownHoverBackColor = value; InvalidateDropDown(); }
    }

    /// <summary>Background color used by the selected drop-down item.</summary>
    [Category("EZControls")]
    public Color DropDownSelectedBackColor
    {
        get => _dropDownSelectedBackColor;
        set { _dropDownSelectedBackColor = value; InvalidateDropDown(); }
    }

    /// <summary>Text color used by the selected drop-down item.</summary>
    [Category("EZControls")]
    public Color DropDownSelectedForeColor
    {
        get => _dropDownSelectedForeColor;
        set { _dropDownSelectedForeColor = value; InvalidateDropDown(); }
    }

    /// <summary>Corner radius for the custom drop-down popup.</summary>
    [Category("EZControls"), DefaultValue(10)]
    public int DropDownBorderRadius
    {
        get => _dropDownBorderRadius;
        set { _dropDownBorderRadius = Math.Max(0, value); InvalidateDropDown(); }
    }

    /// <summary>Where the custom drop-down appears relative to the control.</summary>
    [Category("EZControls"), DefaultValue(EZDropDownPlacement.Bottom)]
    public EZDropDownPlacement DropDownPlacement
    {
        get => _dropDownPlacement;
        set => _dropDownPlacement = value;
    }

    /// <summary>Gap in pixels between the control and its custom drop-down popup.</summary>
    [Category("EZControls"), DefaultValue(7)]
    public int DropDownGap
    {
        get => _dropDownGap;
        set => _dropDownGap = Math.Max(0, value);
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Auto-complete properties
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>Auto-complete mode (None, Append, Suggest, or SuggestAppend).</summary>
    [Category("EZControls"), DefaultValue(AutoCompleteMode.None)]
    public AutoCompleteMode AutoCompleteMode
    {
        get => _autoCompleteMode;
        set
        {
            _autoCompleteMode = value;
            if (_inner is not null)
                _inner.AutoCompleteMode = value;
        }
    }

    /// <summary>Source for auto-complete suggestions (ListItems, History, etc.).</summary>
    [Category("EZControls"), DefaultValue(AutoCompleteSource.ListItems)]
    public AutoCompleteSource AutoCompleteSource
    {
        get => _autoCompleteSource;
        set
        {
            _autoCompleteSource = value;
            if (_inner is not null)
                _inner.AutoCompleteSource = value;
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Accessibility properties
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>Accessible name for screen readers.</summary>
    [Category("EZControls"), DefaultValue("")]
    public string AccessibleControlName
    {
        get => _accessibleName;
        set
        {
            _accessibleName = value;
            if (_inner is not null)
                _inner.AccessibleName = value;
        }
    }

    /// <summary>Accessible description for screen readers.</summary>
    [Category("EZControls"), DefaultValue("")]
    public string AccessibleControlDescription
    {
        get => _accessibleDescription;
        set
        {
            _accessibleDescription = value;
            if (_inner is not null)
                _inner.AccessibleDescription = value;
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Painting
    // ══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    /// 

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        if (Width <= 1 || Height <= 1) return; 
        Graphics g = e.Graphics;
        GraphicsHelper.SetHighQuality(g);

        bool isActive = _isDroppedDown || State == EZControlState.Focused;
        Color activeBorder = isActive ? _borderFocusColor : _borderColor;
        PaintSurface(g, activeBorder);

        if (_inner is null)
            return;

        bool isRtl = RightToLeft == RightToLeft.Yes;
        Rectangle textRect = isRtl
            ? new Rectangle(32, 0, Width - 42, Height)
            : new Rectangle(_borderRadius > 0 ? 10 : 6, 0, Width - 42, Height);


        string displayText = _inner.Text ?? string.Empty;

        if (!string.IsNullOrEmpty(displayText))
        {
            ControlPainter.DrawText(
                g, textRect, displayText, Font, ForeColor,
                isRtl ? ContentAlignment.MiddleRight : ContentAlignment.MiddleLeft);
        }

        DrawChevron(g);
    }

    /// <inheritdoc/>
    protected override void OnPaintBackground(PaintEventArgs pevent)
    {
        // Prevent flicker
    }

    private void PaintSurface(Graphics g, Color borderColor)
    {
        var borderSize = Math.Max(1, _borderSize);
        var halfPen = borderSize / 2f;
        var bounds = new RectangleF(
            halfPen,
            halfPen,
            Width - borderSize,
            Height - borderSize);

        if (bounds.Width <= 0 || bounds.Height <= 0)
            return;

        var radius = Math.Min(
            _borderRadius,
            (int)Math.Floor(Math.Min(bounds.Width, bounds.Height) / 2f));

        var oldSmoothing = g.SmoothingMode;
        var oldPixelOffset = g.PixelOffsetMode;
        var oldCompositing = g.CompositingQuality;

        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.PixelOffsetMode = PixelOffsetMode.Half;
        g.CompositingQuality = CompositingQuality.HighQuality;

        using var path = CreateRoundedPath(bounds, radius);
        using var fillBrush = new SolidBrush(BackColor);
        g.FillPath(fillBrush, path);

        if (borderSize > 0)
        {
            using var pen = new Pen(borderColor, borderSize)
            {
                Alignment = PenAlignment.Center,
                LineJoin = LineJoin.Round,
                StartCap = LineCap.Round,
                EndCap = LineCap.Round
            };

            g.DrawPath(pen, path);
        }

        g.SmoothingMode = oldSmoothing;
        g.PixelOffsetMode = oldPixelOffset;
        g.CompositingQuality = oldCompositing;
    }

    /// <summary>Draws the down-arrow chevron on the right side of the control.</summary>
    private void DrawChevron(Graphics g)
    {
        const int iconW = 12;
        const int iconH = 7;

        bool isRtl = RightToLeft == RightToLeft.Yes;
        int x = isRtl ? 12 : Width - iconW - 12;
        int y = (Height - iconH) / 2;

        using GraphicsPath path = new();
        path.AddLine(x, y, x + iconW / 2, y + iconH);
        path.AddLine(x + iconW / 2, y + iconH, x + iconW, y);

        using Pen pen = new(_iconColor, 2f)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round,
            LineJoin = LineJoin.Round
        };

        g.DrawPath(pen, path);
    }

    private static GraphicsPath CreateRoundedPath(RectangleF bounds, float radius)
    {
        var path = new GraphicsPath();

        if (radius <= 0)
        {
            path.AddRectangle(bounds);
            path.CloseFigure();
            return path;
        }

        var diameter = radius * 2f;
        path.StartFigure();
        path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
        path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
        path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();
        return path;
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Interaction
    // ══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    protected override void OnClick(EventArgs e)
    {
        base.OnClick(e);
    }

    /// <inheritdoc/>
    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (!IsInDesigner)
            OpenDropDown();
    }

    /// <inheritdoc/>
    protected override void OnMouseWheel(MouseEventArgs e)
    {
        base.OnMouseWheel(e);

        if (_inner is null || IsInDesigner || _isDroppedDown)
            return;

        int newIndex = _inner.SelectedIndex + (e.Delta > 0 ? -1 : 1);
        newIndex = Math.Max(-1, Math.Min(newIndex, _inner.Items.Count - 1));

        if (newIndex != _inner.SelectedIndex)
        {
            _inner.SelectedIndex = newIndex;
            ((HandledMouseEventArgs)e).Handled = true;
        }
    }

    private void OpenDropDown()
    {
        if (IsInDesigner || _inner is null || !Enabled)
            return;

        if (_isDroppedDown)
        {
            _dropDown?.Close(ToolStripDropDownCloseReason.CloseCalled);
            return;
        }

        var items = GetDropDownItems().ToList();
        if (items.Count == 0)
            return;

        _dropDown?.Dispose();
        _dropDown = CreateDropDown(items);
        _dropDown.Opened += (_, _) =>
        {
            _isDroppedDown = true;
            State = EZControlState.Focused;
            Invalidate();
        };
        _dropDown.Closed += (_, _) =>
        {
            _isDroppedDown = false;
            State = Focused ? EZControlState.Focused : EZControlState.Normal;
            Invalidate();
        };

        var popupWidth = Math.Max(Width, 180);

        _dropDown.Width = popupWidth;
        foreach (ToolStripItem item in _dropDown.Items)
            item.Width = popupWidth - _dropDown.Padding.Horizontal;

        var screenPoint = GetDropDownScreenPoint(_dropDown, popupWidth);
        _dropDown.Show(screenPoint);
    }

    private Point GetDropDownScreenPoint(ToolStripDropDown dropDown, int popupWidth)
    {
        var popupHeight = Math.Max(dropDown.Height, dropDown.PreferredSize.Height);
        var isRtl = RightToLeft == RightToLeft.Yes;

        var x = _dropDownPlacement switch
        {
            EZDropDownPlacement.Left => -popupWidth - _dropDownGap,
            EZDropDownPlacement.Right => Width + _dropDownGap,
            _ => isRtl ? Width - popupWidth : 0
        };

        var y = _dropDownPlacement switch
        {
            EZDropDownPlacement.Top => -popupHeight - _dropDownGap,
            EZDropDownPlacement.Left or EZDropDownPlacement.Right => 0,
            _ => Height + _dropDownGap
        };

        return PointToScreen(new Point(x, y));
    }

    private IEnumerable<(int Index, string Text)> GetDropDownItems()
    {
        if (_inner is null)
            yield break;

        for (var i = 0; i < _inner.Items.Count; i++)
        {
            var text = _inner.GetItemText(_inner.Items[i]);
            if (!string.IsNullOrWhiteSpace(text))
                yield return (i, text);
        }
    }

    private ToolStripDropDown CreateDropDown(IReadOnlyList<(int Index, string Text)> items)
    {
        var dropDown = new EZComboBoxDropDown(
            _dropDownBackColor,
            _dropDownBorderColor,
            _dropDownBorderRadius)
        {
            RightToLeft = RightToLeft,
            Font = Font
        };

        var selectedIndex = _inner?.SelectedIndex ?? -1;
        var itemHeight = Math.Max(30, _dropDownItemHeight);
        var maxVisibleItems = _maxDropDownItems > 0
            ? _maxDropDownItems
            : Math.Max(1, _dropDownHeight / itemHeight);

        // Every entry is created so long lists stay reachable: the list itself
        // scrolls instead of cutting the overflow away.
        var list = new EZComboBoxDropDownList(
            _dropDownBackColor,
            RightToLeft == RightToLeft.Yes)
        {
            Font = Font
        };

        foreach (var item in items)
        {
            var cell = new EZComboBoxDropDownCell(
                item.Text,
                item.Index == selectedIndex,
                itemHeight,
                _dropDownBackColor,
                _dropDownForeColor,
                _dropDownHoverBackColor,
                _dropDownSelectedBackColor,
                _dropDownSelectedForeColor,
                _dropDownBorderColor)
            {
                RightToLeft = RightToLeft,
                Font = Font
            };

            cell.Click += (_, _) => SelectDropDownItem(item.Index);
            list.AddCell(cell);
        }

        int visibleHeight = Math.Min(items.Count, maxVisibleItems) * itemHeight;

        var host = new ToolStripControlHost(list)
        {
            AutoSize = false,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            Size = new Size(
                Math.Max(140, Width - 12),
                Math.Max(itemHeight, visibleHeight))
        };

        dropDown.Items.Add(host);

        return dropDown;
    }

    private void SelectDropDownItem(int index)
    {
        if (_inner is null || index < 0 || index >= _inner.Items.Count)
            return;

        _inner.SelectedIndex = index;
        _dropDown?.Close(ToolStripDropDownCloseReason.ItemClicked);
        Focus();
        Invalidate();
    }

    private void InvalidateDropDown()
    {
        _dropDown?.Invalidate();
        if (_dropDown is null)
            return;

        foreach (ToolStripItem item in _dropDown.Items)
            item.Invalidate();
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Layout
    // ══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        PositionInnerControl();
    }

    /// <inheritdoc/>
    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        PositionInnerControl();
    }

    /// <inheritdoc/>
    protected override void OnFontChanged(EventArgs e)
    {
        base.OnFontChanged(e);
        if (_inner is not null)
            _inner.Font = Font;
    }

    /// <inheritdoc/>
    protected override void OnForeColorChanged(EventArgs e)
    {
        base.OnForeColorChanged(e);
        if (_inner is not null && !_isDroppedDown)
            _inner.ForeColor = _dropDownForeColor;
        Invalidate();
    }

    /// <inheritdoc/>
    protected override void OnBackColorChanged(EventArgs e)
    {
        base.OnBackColorChanged(e);
        if (_inner is not null)
            _inner.BackColor = _dropDownBackColor;
        Invalidate();
    }

    /// <summary>
    /// Positions the invisible native ComboBox so its drop-down list
    /// appears at the correct location beneath the visual chrome.
    /// </summary>
    private void PositionInnerControl()
    {
        if (_inner is null || Width <= 0 || Height <= 0)
            return;

        _inner.Bounds = new Rectangle(0, Height - 1, Width, 1);
        _inner.DropDownWidth = Width;
    }

    /// <inheritdoc/>
    protected override void OnRightToLeftChanged(EventArgs e)
    {
        base.OnRightToLeftChanged(e);
        _dropDown?.Close(ToolStripDropDownCloseReason.CloseCalled);
        Invalidate();
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Keyboard
    // ══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    protected override bool IsInputKey(Keys keyData) =>
        keyData is Keys.Up or Keys.Down or Keys.Return or Keys.Escape or Keys.Tab
        || base.IsInputKey(keyData);

    /// <inheritdoc/>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (_inner is null)
        {
            base.OnKeyDown(e);
            return;
        }

        base.OnKeyDown(e);

        switch (e.KeyCode)
        {
            case Keys.Down when !_isDroppedDown:
                OpenDropDown();
                e.Handled = true;
                break;

            case Keys.Up when !_isDroppedDown && _inner.SelectedIndex > 0:
                _inner.SelectedIndex--;
                e.Handled = true;
                break;

            case Keys.Down when !_isDroppedDown && _inner.SelectedIndex < _inner.Items.Count - 1:
                _inner.SelectedIndex++;
                e.Handled = true;
                break;

            case Keys.Escape when _isDroppedDown:
                _dropDown?.Close(ToolStripDropDownCloseReason.Keyboard);
                e.Handled = true;
                break;

            case Keys.Tab:
                // Allow Tab to move focus to next control
                e.Handled = false;
                break;
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Theming
    // ══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public override void ApplyTheme(EZTheme theme)
    {
        Font = theme.DefaultFont;
        BackColor = theme.BackgroundColor;
        ForeColor = theme.TextColor;
        _borderColor = theme.BorderColor;
        _borderFocusColor = theme.FocusedColor;
        _iconColor = theme.TextColor;
        _dropDownBackColor = theme.BackgroundColor;
        _dropDownForeColor = theme.TextColor;

        if (_inner is not null)
        {
            _inner.Font = theme.DefaultFont;
            _inner.BackColor = theme.BackgroundColor;
            _inner.ForeColor = theme.TextColor;
        }

        Invalidate();
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Helpers
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>Syncs the Items array to the inner ComboBox.</summary>
    private void SyncItemsToInner()
    {
        if (_inner is null)
            return;

        _inner.Items.Clear();
        if (_items.Length > 0)
            _inner.Items.AddRange(_items);
    }

    private sealed class EZComboBoxDropDown : ToolStripDropDown
    {
        private readonly Color _backColor;
        private readonly Color _borderColor;
        private readonly int _radius;

        public EZComboBoxDropDown(Color backColor, Color borderColor, int radius)
        {
            _backColor = backColor;
            _borderColor = borderColor;
            _radius = radius;

            AutoClose = true;
            AutoSize = true;
            BackColor = backColor;
            Padding = new Padding(6);
            Margin = Padding.Empty;
            Renderer = new EZComboBoxDropDownRenderer();
        }

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            ApplyRoundedRegion();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            ApplyRoundedRegion();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;

            var bounds = new RectangleF(0.5F, 0.5F, Width - 1F, Height - 1F);
            using var path = CreateRoundedPath(bounds, Math.Min(_radius, Math.Min(Width, Height) / 2F));
            using var fillBrush = new SolidBrush(_backColor);
            e.Graphics.FillPath(fillBrush, path);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;

            var bounds = new RectangleF(0.5F, 0.5F, Width - 1F, Height - 1F);
            using var path = CreateRoundedPath(bounds, Math.Min(_radius, Math.Min(Width, Height) / 2F));
            using var pen = new Pen(_borderColor);
            e.Graphics.DrawPath(pen, path);
        }

        private void ApplyRoundedRegion()
        {
            if (Width <= 0 || Height <= 0)
                return;

            using var path = CreateRoundedPath(
                new RectangleF(0, 0, Width, Height),
                Math.Min(_radius, Math.Min(Width, Height) / 2F));

            Region?.Dispose();
            Region = new Region(path);
        }
    }

    /// <summary>
    /// Scrollable host for the drop-down entries. A plain panel is used instead
    /// of tool-strip items so long lists (countries, time zones) can be reached
    /// with the scroll bar instead of being cut off.
    /// </summary>
    private sealed class EZComboBoxDropDownList : Panel
    {
        private int _nextY;

        public EZComboBoxDropDownList(Color backColor, bool rightToLeft)
        {
            AutoScroll = true;
            BackColor = backColor;
            DoubleBuffered = true;
            Padding = Padding.Empty;
            Margin = Padding.Empty;
            RightToLeft = rightToLeft ? RightToLeft.Yes : RightToLeft.No;
        }

        public void AddCell(EZComboBoxDropDownCell cell)
        {
            cell.Location = new Point(0, _nextY);
            _nextY += cell.Height;
            Controls.Add(cell);
            ResizeCells();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            ResizeCells();
        }

        private void ResizeCells()
        {
            int width = ClientSize.Width;
            foreach (Control child in Controls)
                child.Width = width;
        }
    }

    private sealed class EZComboBoxDropDownCell : Control
    {
        private readonly bool _isSelected;
        private readonly Color _backColor;
        private readonly Color _foreColor;
        private readonly Color _hoverBackColor;
        private readonly Color _selectedBackColor;
        private readonly Color _selectedForeColor;
        private readonly Color _borderColor;
        private bool _hovered;

        public EZComboBoxDropDownCell(
            string text,
            bool isSelected,
            int height,
            Color backColor,
            Color foreColor,
            Color hoverBackColor,
            Color selectedBackColor,
            Color selectedForeColor,
            Color borderColor)
        {
            Text = text;
            _isSelected = isSelected;
            _backColor = backColor;
            _foreColor = foreColor;
            _hoverBackColor = hoverBackColor;
            _selectedBackColor = selectedBackColor;
            _selectedForeColor = selectedForeColor;
            _borderColor = borderColor;

            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.UserPaint |
                ControlStyles.ResizeRedraw,
                true);

            Height = height;
            Cursor = Cursors.Hand;
            BackColor = backColor;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _hovered = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hovered = false;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.Half;

            var isRtl = RightToLeft == RightToLeft.Yes;
            var hovered = _hovered;
            var bounds = new Rectangle(Point.Empty, Size);
            var content = new Rectangle(5, 3, Width - 10, Height - 6);

            using (var backBrush = new SolidBrush(_backColor))
                g.FillRectangle(backBrush, bounds);

            if (hovered || _isSelected)
            {
                var fill = _isSelected ? _selectedBackColor : _hoverBackColor;
                using var itemPath = CreateRoundedPath(content, 8);
                using var itemBrush = new SolidBrush(fill);
                g.FillPath(itemBrush, itemPath);
            }

            var markerRect = isRtl
                ? new Rectangle(Width - 34, content.Y + (content.Height - 22) / 2, 22, 22)
                : new Rectangle(12, content.Y + (content.Height - 22) / 2, 22, 22);

            DrawMarker(g, markerRect);

            var textRect = isRtl
                ? new Rectangle(12, content.Y, Width - 52, content.Height)
                : new Rectangle(42, content.Y, Width - 54, content.Height);

            var textColor = _isSelected ? _selectedForeColor : _foreColor;
            var flags = TextFormatFlags.VerticalCenter |
                TextFormatFlags.EndEllipsis |
                TextFormatFlags.NoPrefix |
                (isRtl ? TextFormatFlags.Right | TextFormatFlags.RightToLeft : TextFormatFlags.Left);

            TextRenderer.DrawText(g, Text, Font, textRect, textColor, flags);
        }

        private void DrawMarker(Graphics g, Rectangle rect)
        {
            var markerFill = _isSelected ? _selectedForeColor : Color.FromArgb(246, 249, 250);
            var markerBorder = _isSelected ? _selectedForeColor : _borderColor;

            using var path = CreateRoundedPath(rect, 7);
            using var markerBrush = new SolidBrush(markerFill);
            using var markerPen = new Pen(markerBorder);
            g.FillPath(markerBrush, path);
            g.DrawPath(markerPen, path);

            if (!_isSelected)
                return;

            using var checkPen = new Pen(_selectedBackColor, 2F)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round,
                LineJoin = LineJoin.Round
            };

            g.DrawLines(
                checkPen,
                new[]
                {
                    new Point(rect.X + 6, rect.Y + 11),
                    new Point(rect.X + 10, rect.Y + 15),
                    new Point(rect.X + 16, rect.Y + 7)
                });
        }
    }

    private sealed class EZComboBoxDropDownRenderer : ToolStripProfessionalRenderer
    {
        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
        }

        protected override void OnRenderImageMargin(ToolStripRenderEventArgs e)
        {
        }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Dispose
    // ══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _inner?.Dispose();
            _inner = null;
        }

        base.Dispose(disposing);
    }
}
