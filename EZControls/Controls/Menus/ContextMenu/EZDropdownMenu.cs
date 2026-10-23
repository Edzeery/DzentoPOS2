using System.ComponentModel;
using System.Drawing.Drawing2D;
using EZControls.Core.Constants;
using EZControls.Core.Themes;

namespace EZControls.Controls.Menus.ContextMenu;

// ══════════════════════════════════════════════════════════════════════════════
//  Menu System
//
//  Three classes in one file because they form a tightly-coupled triad:
//
//  EZMenuColorTable  – Supplies colors to the ToolStripRenderer pipeline.
//                      Subclasses ProfessionalColorTable so only the colors
//                      we actually care about need to be overridden.
//
//  EZMenuRenderer    – Subclasses ToolStripProfessionalRenderer to swap in
//                      our color table and override item painting for
//                      rounded selection rectangles.
//
//  EZDropdownMenu    – Thin wrapper around ContextMenuStrip that applies
//                      EZMenuRenderer and keeps a clean public API for
//                      adding items programmatically or via the designer.
//
//  Usage:
//      var menu = new EZDropdownMenu();
//      menu.AddItem("Copy",  Icons.Copy,  () => OnCopy());
//      menu.AddItem("Paste", Icons.Paste, () => OnPaste());
//      menu.ShowAt(button, button.Width, 0);
// ══════════════════════════════════════════════════════════════════════════════

// ──────────────────────────────────────────────────────────────────────────────
//  EZMenuColorTable
// ──────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Provides the color palette used by <see cref="EZMenuRenderer"/>.
/// Holds mutable color properties so the table can be updated when the
/// active theme changes.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="ProfessionalColorTable.UseSystemColors"/> is <see langword="false"/>
/// by default in this subclass so that every <c>override</c> below takes effect.
/// It cannot be set via a property override because the base member is not
/// virtual; it is instead forced to <see langword="false"/> by calling
/// <see cref="ProfessionalColorTable.UseSystemColors"/> through the base
/// constructor chain — which always produces <see langword="false"/> when the
/// derived class supplies its own color values.
/// </para>
/// <para>
/// The local palette property is named <c>MenuBorderColor</c> (not
/// <c>MenuBorder</c>) to avoid a name collision with the base-class virtual
/// property <see cref="ProfessionalColorTable.MenuBorder"/> that we override.
/// </para>
/// </remarks>
public sealed class EZMenuColorTable : ProfessionalColorTable
{
    // ── Local palette (mutable; updated by ApplyTheme) ─────────────────────
    // NOTE: named *Color to avoid clashing with the base virtual properties.
    public Color MenuBackgroundColor { get; set; } = Color.White;
    public Color MenuBorderColor { get; set; } = EZDefaultColors.Border;
    public Color ItemSelectedColor { get; set; } = EZDefaultColors.PrimaryFocus;
    public Color ItemSelectedBorder { get; set; } = EZDefaultColors.PrimaryFocus;
    public Color ItemText { get; set; } = EZDefaultColors.Text;
    public Color SeparatorColor { get; set; } = EZDefaultColors.Border;

    // ── ProfessionalColorTable virtual overrides ───────────────────────────
    // UseSystemColors is NOT virtual on ProfessionalColorTable, so we cannot
    // override it. The base class returns false by default when a derived class
    // provides its own colors, so no action is required here.

    public override Color MenuBorder => MenuBorderColor;
    public override Color MenuItemBorder => ItemSelectedBorder;
    public override Color MenuItemSelected => Color.FromArgb(30, ItemSelectedColor);
    public override Color MenuStripGradientBegin => MenuBackgroundColor;
    public override Color MenuStripGradientEnd => MenuBackgroundColor;
    public override Color ToolStripDropDownBackground => MenuBackgroundColor;
    public override Color ImageMarginGradientBegin => MenuBackgroundColor;
    public override Color ImageMarginGradientMiddle => MenuBackgroundColor;
    public override Color ImageMarginGradientEnd => MenuBackgroundColor;
}

// ──────────────────────────────────────────────────────────────────────────────
//  EZMenuRenderer
// ──────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Custom <see cref="ToolStripRenderer"/> that paints rounded menu items,
/// styled backgrounds, and themed separators.
/// </summary>
public sealed class EZMenuRenderer : ToolStripProfessionalRenderer
{
    private readonly EZMenuColorTable _table;
    private readonly int _itemRadius;

    /// <param name="table">Color table to use.</param>
    /// <param name="itemRadius">Corner radius for selected item highlight.</param>
    public EZMenuRenderer(EZMenuColorTable table, int itemRadius = 6)
        : base(table)
    {
        _table = table;
        _itemRadius = itemRadius;
    }

    // ── Item background ───────────────────────────────────────────────────

    /// <inheritdoc/>
    protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
    {
        if (!e.Item.Selected)
            return;

        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        Rectangle rect = Rectangle.Inflate(e.Item.ContentRectangle, -2, -2);

        if (rect.Width <= 0 || rect.Height <= 0) return;

        using GraphicsPath path = CreateRoundedRect(rect, _itemRadius);
        using SolidBrush brush = new(Color.FromArgb(35, _table.ItemSelectedColor));
        g.FillPath(brush, path);

        using Pen pen = new(Color.FromArgb(80, _table.ItemSelectedBorder), 1f);
        g.DrawPath(pen, path);
    }

    // ── Separator ─────────────────────────────────────────────────────────

    /// <inheritdoc/>
    protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
    {
        Graphics g = e.Graphics;
        int y = e.Item.Height / 2;

        using Pen pen = new(_table.SeparatorColor, 1f);
        g.DrawLine(pen, 8, y, e.Item.Width - 8, y);
    }

    // ── Tool strip background ─────────────────────────────────────────────

    /// <inheritdoc/>
    protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
    {
        using SolidBrush bg = new(_table.MenuBackgroundColor);
        e.Graphics.FillRectangle(bg, e.AffectedBounds);
    }

    // ── Tool strip border ─────────────────────────────────────────────────

    /// <inheritdoc/>
    protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
    {
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        Rectangle rect = e.AffectedBounds;
        rect.Width--;
        rect.Height--;

        using GraphicsPath path = CreateRoundedRect(rect, 8);
        using Pen pen = new(_table.MenuBorderColor, 1f);
        g.DrawPath(pen, path);
    }

    // ── Image margin ──────────────────────────────────────────────────────

    /// <inheritdoc/>
    protected override void OnRenderImageMargin(ToolStripRenderEventArgs e)
    {
        // Do not render the image margin stripe; we use a flat look.
    }

    // ── Helper ────────────────────────────────────────────────────────────

    private static GraphicsPath CreateRoundedRect(Rectangle r, int radius)
    {
        int d = radius * 2;
        GraphicsPath path = new();
        path.AddArc(r.X, r.Y, d, d, 180, 90);
        path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
        path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
        path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }
}

// ──────────────────────────────────────────────────────────────────────────────
//  EZDropdownMenu
// ──────────────────────────────────────────────────────────────────────────────

/// <summary>
/// A styled context menu that applies <see cref="EZMenuRenderer"/> and
/// exposes a fluent API for adding items.
/// </summary>
[ToolboxItem(false)]
public sealed class EZDropdownMenu : ContextMenuStrip
{
    private readonly EZMenuColorTable _colorTable;

    // ══════════════════════════════════════════════════════════════════════════
    //  Constructor
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>Initialises the menu with default colors and renderer.</summary>
    public EZDropdownMenu()
    {
        _colorTable = new EZMenuColorTable();

        Renderer = new EZMenuRenderer(_colorTable);
        ShowImageMargin = false;
        ShowCheckMargin = false;
        BackColor = _colorTable.MenuBackgroundColor;
        ForeColor = _colorTable.ItemText;
        Font = EZDefaults.DefaultFont;
        Padding = new Padding(4, 4, 4, 4);
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Fluent API
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Adds a clickable menu item.
    /// </summary>
    /// <param name="text">Display text.</param>
    /// <param name="onClick">Action invoked when the item is clicked.</param>
    /// <returns>The created <see cref="ToolStripMenuItem"/> for further configuration.</returns>
    public ToolStripMenuItem AddItem(string text, Action? onClick = null)
    {
        var item = new ToolStripMenuItem(text)
        {
            ForeColor = _colorTable.ItemText,
            BackColor = Color.Transparent,
            Font = Font
        };

        if (onClick is not null)
            item.Click += (_, _) => onClick();

        Items.Add(item);
        return item;
    }

    /// <summary>
    /// Adds a clickable menu item with an icon.
    /// </summary>
    /// <param name="text">Display text.</param>
    /// <param name="icon">Icon displayed to the left of the text.  May be null.</param>
    /// <param name="onClick">Action invoked when the item is clicked.</param>
    /// <returns>The created <see cref="ToolStripMenuItem"/>.</returns>
    public ToolStripMenuItem AddItem(string text, Image? icon, Action? onClick = null)
    {
        var item = AddItem(text, onClick);
        item.Image = icon;
        return item;
    }

    /// <summary>Adds a separator line between groups of items.</summary>
    /// <returns>The created <see cref="ToolStripSeparator"/>.</returns>
    public ToolStripSeparator AddSeparator()
    {
        var sep = new ToolStripSeparator();
        Items.Add(sep);
        return sep;
    }

    /// <summary>
    /// Shows the menu at the specified offset relative to <paramref name="control"/>.
    /// </summary>
    public void ShowAt(Control control, int offsetX = 0, int offsetY = 0)
    {
        Point location = control.PointToScreen(new Point(offsetX, offsetY));
        Show(location);
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Theming
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>Applies an <see cref="EZTheme"/> to the menu chrome and all current items.</summary>
    public void ApplyTheme(EZTheme theme)
    {
        _colorTable.MenuBackgroundColor = theme.SurfaceColor;
        _colorTable.MenuBorderColor = theme.BorderColor;
        _colorTable.ItemSelectedColor = theme.PrimaryColor;
        _colorTable.ItemSelectedBorder = theme.PrimaryColor;
        _colorTable.ItemText = theme.TextColor;
        _colorTable.SeparatorColor = theme.BorderColor;

        BackColor = theme.SurfaceColor;
        ForeColor = theme.TextColor;
        Font = theme.DefaultFont;

        foreach (ToolStripItem item in Items)
        {
            item.ForeColor = theme.TextColor;
            item.BackColor = Color.Transparent;
            item.Font = theme.DefaultFont;
        }

        // Force the renderer to repaint with updated colors.
        Renderer = new EZMenuRenderer(_colorTable);
    }
}