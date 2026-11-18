using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using DzentoPOS2.App.Services.Sales;
using DzentoPOS2.Core.Configuration;
using DzentoPOS2.Localization.Resources;
using DzentoPOS2.Shared.Helpers;
using DzentoPOS2.WinForms.Themes;

namespace DzentoPOS2.WinForms.Controls.Pos;

/// <summary>
/// Virtualized, owner-drawn product list for the POS screen. Only the visible
/// cards are painted, and every font, brush and pen is created once instead of
/// per paint, so scrolling a 5,000-variant catalog stays cheap. The control is
/// safe on a design surface: it starts no timer, loads no image and subscribes
/// to no theme event there.
/// </summary>
public class PosProductList : Control
{
    private const int ImageCacheCapacity = 64;

    private static readonly string ImagesRoot = Path.Combine(AppPaths.Root, "images");

    // Fonts never change with the theme and are shared by every instance.
    private static readonly Font EmptyFont = new("Segoe UI", 12F);
    private static readonly Font NameFont = new("Segoe UI", 10F, FontStyle.Bold);
    private static readonly Font SkuFont = new("Segoe UI", 8F);
    private static readonly Font PriceFont = new("Segoe UI", 10.5F, FontStyle.Bold);
    private static readonly Font BadgeFont = new("Segoe UI", 8F, FontStyle.Bold);
    private static readonly Font BarcodeFont = new("Segoe UI", 7.5F, FontStyle.Bold);
    private static readonly Font AddFont = new("Segoe UI", 12F, FontStyle.Bold);
    private static readonly Font InitialFont = new("Segoe UI", 16F, FontStyle.Bold);

    private readonly VScrollBar _vScroll = new() { Width = 16, TabStop = false, Visible = false };
    private readonly Dictionary<string, Image> _imageCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _imageLoads = new(StringComparer.OrdinalIgnoreCase);
    private readonly LinkedList<string> _lru = new();
    private readonly object _imageLock = new();

    private IReadOnlyList<PosProductItem> _items = Array.Empty<PosProductItem>();
    private PosViewMode _mode = PosViewMode.Cards;
    private int _selectedIndex = -1;
    private int _scrollOffset;
    private int _hoverIndex = -1;
    private bool _themeSubscribed;
    private readonly bool _designMode;

    private Brush _backgroundBrush = Brushes.Transparent;
    private Brush _surfaceBrush = Brushes.White;
    private Brush _hoverBrush = Brushes.White;
    private Brush _selectionBrush = Brushes.White;
    private Brush _surfaceAltBrush = Brushes.White;
    private Brush _primaryBrush = Brushes.Teal;
    private Brush _primarySoftBrush = Brushes.White;
    private Brush _dangerBrush = Brushes.Red;
    private Brush _warningBrush = Brushes.Orange;
    private Brush _onDarkBrush = Brushes.White;
    private Pen _borderPen = new(Color.Gray);
    private Pen _primaryPen = new(Color.Teal);

    /// <summary>Raised when a card is clicked once.</summary>
    public event EventHandler<PosProductItem>? ItemClicked;

    /// <summary>Raised when a card is activated (double-click, Enter or the + button).</summary>
    public event EventHandler<PosProductItem>? ItemDoubleClicked;

    public PosProductList()
    {
        SetStyle(
            ControlStyles.OptimizedDoubleBuffer
            | ControlStyles.AllPaintingInWmPaint
            | ControlStyles.UserPaint
            | ControlStyles.ResizeRedraw
            | ControlStyles.Selectable
            | ControlStyles.StandardClick
            | ControlStyles.StandardDoubleClick,
            true);

        TabStop = true;
        _designMode = DesignTime.IsActive(this);

        _vScroll.Scroll += OnScrollBarScroll;
        Controls.Add(_vScroll);

        RebuildThemeResources();

        if (!_designMode)
        {
            ThemeManager.ThemeChanged += OnThemeChanged;
            _themeSubscribed = true;
        }

        MouseMove += OnMouseMove;
        MouseLeave += OnMouseLeave;
        MouseClick += OnMouseClick;
        MouseDoubleClick += OnMouseDoubleClick;
        KeyDown += OnKeyDown;
    }

    /// <summary>The filtered catalog shown by the list.</summary>
    public IReadOnlyList<PosProductItem> Items
    {
        get => _items;
        set
        {
            _items = value ?? Array.Empty<PosProductItem>();
            _selectedIndex = -1;
            _hoverIndex = -1;
            _scrollOffset = 0;
            UpdateScrollBar();
            Invalidate();
        }
    }

    /// <summary>Cards (default) or a compact single-column list.</summary>
    public PosViewMode ViewMode
    {
        get => _mode;
        set
        {
            if (_mode == value) return;
            _mode = value;
            UpdateScrollBar();
            Invalidate();
        }
    }

    /// <summary>Vertical offset in pixels, clamped to the content height.</summary>
    public int ScrollOffset
    {
        get => _scrollOffset;
        set
        {
            int clamped = Math.Clamp(value, 0, MaxOffset);
            if (clamped == _scrollOffset) return;

            _scrollOffset = clamped;
            _vScroll.Value = clamped;
            Invalidate();
        }
    }

    /// <summary>The keyboard/click selection, or null when nothing is selected.</summary>
    public PosProductItem? SelectedItem =>
        _selectedIndex >= 0 && _selectedIndex < _items.Count ? _items[_selectedIndex] : null;

    public int SelectedIndex => _selectedIndex;

    public bool IsRightToLeftMode => RightToLeft == RightToLeft.Yes;

    private int ViewportWidth =>
        Math.Max(1, ClientSize.Width - (_vScroll.Visible ? _vScroll.Width : 0));

    private int MaxOffset => Math.Max(0, TotalHeight - ClientSize.Height);

    private int TotalHeight
    {
        get
        {
            var (cols, _, itemHeight, gap) = Grid;
            int rows = (_items.Count + cols - 1) / cols;
            return rows * (itemHeight + gap);
        }
    }

    private (int Columns, int ItemWidth, int ItemHeight, int Gap) Grid =>
        PosCardLayoutCalculator.GetGrid(ViewportWidth, _mode);

    // =========================
    // Scrolling
    // =========================

    private void OnScrollBarScroll(object? sender, ScrollEventArgs e)
    {
        _scrollOffset = _vScroll.Value;
        Invalidate();
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        base.OnMouseWheel(e);

        if (!_vScroll.Visible) return;
        ScrollOffset = _scrollOffset - e.Delta;
    }

    private void UpdateScrollBar()
    {
        var (_, _, itemHeight, _) = Grid;
        int viewport = Math.Max(1, ClientSize.Height);

        bool needed = TotalHeight > viewport;
        if (_vScroll.Visible != needed)
            _vScroll.Visible = needed;

        // The visible width changed with the scroll bar, so the content height
        // is measured again before the scroll range is published.
        int content = TotalHeight;

        // Maximum is the last addressable pixel; the reachable last value is
        // Maximum - LargeChange + 1, which must equal content - viewport.
        _vScroll.Maximum = Math.Max(0, content - 1);
        _vScroll.LargeChange = Math.Max(1, Math.Min(viewport, Math.Max(1, content)));
        _vScroll.SmallChange = Math.Max(1, itemHeight);

        _scrollOffset = Math.Clamp(_scrollOffset, 0, Math.Max(0, content - viewport));
        _vScroll.Value = _scrollOffset;

        LayoutScrollBar();
    }

    /// <summary>
    /// The scroll bar is positioned by hand because WinForms mirrors a docked
    /// control in RTL, which would hide it under the cards.
    /// </summary>
    private void LayoutScrollBar()
    {
        int width = _vScroll.Width;
        int x = IsRightToLeftMode ? 0 : Math.Max(0, ClientSize.Width - width);

        _vScroll.Bounds = new Rectangle(x, 0, width, Math.Max(0, ClientSize.Height));
    }

    private void EnsureVisible(int index)
    {
        if (index < 0) return;

        var (cols, _, itemHeight, gap) = Grid;
        int row = index / cols;
        int top = row * (itemHeight + gap);
        int bottom = top + itemHeight;

        if (top < _scrollOffset)
            ScrollOffset = top;
        else if (bottom > _scrollOffset + ClientSize.Height)
            ScrollOffset = bottom - ClientSize.Height;
    }

    // =========================
    // Painting
    // =========================

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var (cols, itemWidth, itemHeight, gap) = Grid;
        bool rtl = IsRightToLeftMode;
        int originX = rtl && _vScroll.Visible ? _vScroll.Width : 0;

        g.FillRectangle(_backgroundBrush, ClientRectangle);

        if (_items.Count == 0)
        {
            TextRenderer.DrawText(
                g,
                AppStrings.NoDataFound,
                EmptyFont,
                ClientRectangle,
                AppColors.InkMuted,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            return;
        }

        int rowStep = itemHeight + gap;
        int firstRow = Math.Max(0, _scrollOffset / rowStep);
        int visibleRows = ClientSize.Height / rowStep + 2;

        int firstIndex = firstRow * cols;
        int lastIndex = Math.Min(_items.Count, firstIndex + visibleRows * cols);

        for (int i = firstIndex; i < lastIndex; i++)
        {
            int row = i / cols;
            int col = i % cols;
            int x = rtl
                ? originX + ViewportWidth - (col + 1) * (itemWidth + gap) + gap
                : originX + col * (itemWidth + gap);
            int y = row * rowStep - _scrollOffset;

            var cardRect = new Rectangle(x, y, itemWidth, itemHeight);
            if (cardRect.Bottom < 0 || cardRect.Top > ClientSize.Height)
                continue;

            DrawCard(g, _items[i], cardRect, i == _selectedIndex, i == _hoverIndex, rtl);
        }
    }

    private void DrawCard(
        Graphics g,
        PosProductItem item,
        Rectangle cardRect,
        bool selected,
        bool hovered,
        bool rtl)
    {
        Brush cardBrush = selected ? _selectionBrush : hovered ? _hoverBrush : _surfaceBrush;

        using (GraphicsPath cardPath = PosDrawing.RoundedRect(cardRect, 8))
        {
            g.FillPath(cardBrush, cardPath);
            g.DrawPath(selected ? _primaryPen : _borderPen, cardPath);
        }

        PosCardRects local = PosCardLayoutCalculator.Calculate(cardRect.Width, cardRect.Height, rtl);
        var rects = local with
        {
            Image = Offset(local.Image, cardRect),
            Name = Offset(local.Name, cardRect),
            Sku = Offset(local.Sku, cardRect),
            Price = Offset(local.Price, cardRect),
            Badge = Offset(local.Badge, cardRect),
            AddButton = Offset(local.AddButton, cardRect)
        };

        DrawImage(g, item, rects.Image);
        DrawName(g, item, rects.Name, rtl);
        DrawSku(g, item, rects.Sku, rtl);
        DrawPrice(g, item, rects.Price, rtl);
        DrawStockBadge(g, item, rects.Badge);
        DrawAddButton(g, rects.AddButton);

        if (selected && Focused)
        {
            var focus = Rectangle.Inflate(cardRect, -3, -3);
            ControlPaint.DrawFocusRectangle(g, focus, AppColors.Ink, Color.Empty);
        }
    }

    private void DrawImage(Graphics g, PosProductItem item, Rectangle rect)
    {
        Image? image = GetCachedImage(item.ThumbnailPath ?? item.ImagePath);

        if (image is not null)
        {
            InterpolationMode previous = g.InterpolationMode;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.DrawImage(image, rect);
            g.InterpolationMode = previous;
            return;
        }

        g.FillRectangle(_surfaceAltBrush, rect);

        // Unified fallback: the product's initial, never an empty box.
        string initial = string.IsNullOrWhiteSpace(item.Name)
            ? "?"
            : item.Name.Trim()[0].ToString();

        TextRenderer.DrawText(
            g,
            initial,
            InitialFont,
            rect,
            AppColors.InkMuted,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }

    private void DrawName(Graphics g, PosProductItem item, Rectangle rect, bool rtl)
    {
        TextRenderer.DrawText(
            g,
            string.IsNullOrWhiteSpace(item.Name) ? "-" : item.Name,
            NameFont,
            rect,
            AppColors.Ink,
            TextFormatFlags.WordBreak
            | TextFormatFlags.EndEllipsis
            | TextFormatFlags.NoPrefix
            | (rtl ? TextFormatFlags.RightToLeft | TextFormatFlags.Right : TextFormatFlags.Left));
    }

    private void DrawSku(Graphics g, PosProductItem item, Rectangle rect, bool rtl)
    {
        bool hasSku = !string.IsNullOrWhiteSpace(item.SKU);
        bool hasBarcodeBadge = item.BarcodeCount > 1;

        if (!hasSku && !hasBarcodeBadge)
            return;

        Rectangle textRect = rect;
        Rectangle? barcodeRect = null;

        // Multi-barcode products get a small pill with a barcode glyph and the
        // number of codes, so the cashier can see the scan behaves differently.
        if (hasBarcodeBadge)
        {
            const int pillWidth = 38;
            int pillX = rtl ? rect.X : rect.Right - pillWidth;

            barcodeRect = new Rectangle(pillX, rect.Y, pillWidth, rect.Height);
            textRect = rtl
                ? new Rectangle(rect.X + pillWidth + 4, rect.Y, Math.Max(0, rect.Width - pillWidth - 4), rect.Height)
                : new Rectangle(rect.X, rect.Y, Math.Max(0, rect.Width - pillWidth - 4), rect.Height);
        }

        if (hasSku)
        {
            TextRenderer.DrawText(
                g,
                item.SKU!,
                SkuFont,
                textRect,
                AppColors.InkMuted,
                TextFormatFlags.EndEllipsis
                | TextFormatFlags.VerticalCenter
                | TextFormatFlags.NoPrefix
                | (rtl ? TextFormatFlags.RightToLeft | TextFormatFlags.Right : TextFormatFlags.Left));
        }

        if (barcodeRect is Rectangle pill)
            DrawBarcodePill(g, pill, item.BarcodeCount);
    }

    private void DrawBarcodePill(Graphics g, Rectangle rect, int count)
    {
        using (GraphicsPath path = PosDrawing.RoundedRect(rect, 4))
            g.FillPath(_primarySoftBrush, path);

        // Four short bars read as a barcode at 8 px tall.
        int barsY = rect.Y + 4;
        int barsHeight = Math.Max(5, rect.Height - 9);
        int x = rect.X + 5;

        for (int i = 0; i < 4; i++)
        {
            int barWidth = i % 2 == 0 ? 2 : 1;
            g.FillRectangle(_primaryBrush, x, barsY, barWidth, barsHeight);
            x += barWidth + 1;
        }

        TextRenderer.DrawText(
            g,
            count.ToString(CultureInfo.InvariantCulture),
            BarcodeFont,
            new Rectangle(x + 2, rect.Y, Math.Max(0, rect.Right - x - 4), rect.Height),
            AppColors.Primary,
            TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.NoPrefix);
    }

    private void DrawPrice(Graphics g, PosProductItem item, Rectangle rect, bool rtl)
    {
        string text = KpiValueFormatter.Format(
            item.SalePrice,
            KpiValueKind.Money,
            CultureInfo.CurrentCulture);

        TextRenderer.DrawText(
            g,
            text,
            PriceFont,
            rect,
            AppColors.Primary,
            TextFormatFlags.VerticalCenter
            | TextFormatFlags.NoPrefix
            | (rtl ? TextFormatFlags.RightToLeft | TextFormatFlags.Left : TextFormatFlags.Right));
    }

    private void DrawStockBadge(Graphics g, PosProductItem item, Rectangle rect)
    {
        if (item.Stock >= 5)
            return;

        bool outOfStock = item.Stock <= 0;
        string text = outOfStock ? "Out" : item.Stock.ToString("0", CultureInfo.CurrentCulture);

        g.FillRectangle(outOfStock ? _dangerBrush : _warningBrush, rect);

        TextRenderer.DrawText(
            g,
            text,
            BadgeFont,
            rect,
            outOfStock ? AppColors.OnDark : Color.FromArgb(48, 33, 0),
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
    }

    private void DrawAddButton(Graphics g, Rectangle rect)
    {
        using GraphicsPath path = PosDrawing.RoundedRect(rect, 4);
        g.FillPath(_primaryBrush, path);
        g.DrawString("+", AddFont, _onDarkBrush, rect, CenteredFormat);
    }

    private static readonly StringFormat CenteredFormat = new()
    {
        Alignment = StringAlignment.Center,
        LineAlignment = StringAlignment.Center
    };

    private static Rectangle Offset(Rectangle rect, Rectangle card) =>
        new(card.X + rect.X, card.Y + rect.Y, rect.Width, rect.Height);

    // =========================
    // Images
    // =========================

    private Image? GetCachedImage(string? path)
    {
        // A design surface must not start file IO.
        if (_designMode) return null;

        string? full = PosImagePath.Resolve(ImagesRoot, path);
        if (full is null) return null;

        lock (_imageLock)
        {
            if (_imageCache.TryGetValue(full, out Image? cached))
            {
                TouchLru(full);
                return cached;
            }

            // One load per file: a repaint storm must not queue dozens of reads.
            if (!_imageLoads.Add(full))
                return null;
        }

        _ = LoadImageAsync(full);
        return null;
    }

    private async System.Threading.Tasks.Task LoadImageAsync(string full)
    {
        try
        {
            Image? loaded = await System.Threading.Tasks.Task.Run(() =>
            {
                if (!File.Exists(full)) return null;

                using var stream = new FileStream(full, FileMode.Open, FileAccess.Read, FileShare.Read);
                using Image source = Image.FromStream(stream);
                return (Image)new Bitmap(source);
            }).ConfigureAwait(false);

            lock (_imageLock)
            {
                _imageLoads.Remove(full);

                if (loaded is null)
                    return;

                if (_imageCache.ContainsKey(full))
                {
                    loaded.Dispose();
                    return;
                }

                if (_imageCache.Count >= ImageCacheCapacity)
                    EvictOldestLocked();

                _imageCache[full] = loaded;
                TouchLru(full);
            }

            if (IsHandleCreated && !IsDisposed)
                BeginInvoke(new Action(Invalidate));
        }
        catch (Exception)
        {
            lock (_imageLock)
                _imageLoads.Remove(full);
            // Best effort: a missing or corrupt thumbnail only loses the image.
        }
    }

    private void TouchLru(string key)
    {
        if (_lru.Contains(key))
            _lru.Remove(key);

        _lru.AddFirst(key);
    }

    private void EvictOldestLocked()
    {
        LinkedListNode<string>? last = _lru.Last;
        if (last is null) return;

        _lru.RemoveLast();
        if (_imageCache.TryGetValue(last.Value, out Image? old))
        {
            old.Dispose();
            _imageCache.Remove(last.Value);
        }
    }

    // =========================
    // Input
    // =========================

    private void OnMouseMove(object? sender, MouseEventArgs e)
    {
        int index = HitTest(e.Location);
        if (index == _hoverIndex) return;

        _hoverIndex = index;
        Invalidate();
    }

    private void OnMouseLeave(object? sender, EventArgs e)
    {
        if (_hoverIndex == -1) return;

        _hoverIndex = -1;
        Invalidate();
    }

    private void OnMouseClick(object? sender, MouseEventArgs e)
    {
        int index = HitTest(e.Location);
        if (index < 0) return;

        _selectedIndex = index;
        Focus();

        // The second click of a double-click is handled by OnMouseDoubleClick.
        if (e.Clicks == 1)
        {
            if (IsInsideAddButton(index, e.Location))
                Activate(index);
            else
                ItemClicked?.Invoke(this, _items[index]);
        }

        Invalidate();
    }

    private void OnMouseDoubleClick(object? sender, MouseEventArgs e)
    {
        int index = HitTest(e.Location);
        if (index < 0) return;

        _selectedIndex = index;

        // The add button already activated on the first click.
        if (!IsInsideAddButton(index, e.Location))
            Activate(index);

        Invalidate();
    }

    private void Activate(int index)
    {
        if (index < 0 || index >= _items.Count) return;
        ItemDoubleClicked?.Invoke(this, _items[index]);
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (_items.Count == 0) return;

        var (cols, _, itemHeight, gap) = Grid;
        int step = Math.Max(1, itemHeight + gap);
        int page = Math.Max(cols, (ClientSize.Height / step) * cols);
        int current = _selectedIndex < 0 ? 0 : _selectedIndex;

        switch (e.KeyCode)
        {
            case Keys.Left:
                _selectedIndex = Math.Max(0, current - 1);
                break;
            case Keys.Right:
                _selectedIndex = Math.Min(_items.Count - 1, current + 1);
                break;
            case Keys.Up:
                _selectedIndex = Math.Max(0, current - cols);
                break;
            case Keys.Down:
                _selectedIndex = Math.Min(_items.Count - 1, current + cols);
                break;
            case Keys.PageUp:
                _selectedIndex = Math.Max(0, current - page);
                break;
            case Keys.PageDown:
                _selectedIndex = Math.Min(_items.Count - 1, current + page);
                break;
            case Keys.Home:
                _selectedIndex = 0;
                break;
            case Keys.End:
                _selectedIndex = _items.Count - 1;
                break;
            case Keys.Enter:
            case Keys.Space:
                e.Handled = true;
                e.SuppressKeyPress = true;
                Activate(current);
                EnsureVisible(current);
                Invalidate();
                return;
            default:
                return;
        }

        e.Handled = true;
        EnsureVisible(_selectedIndex);
        Invalidate();
    }

    private int HitTest(Point point)
    {
        var (cols, itemWidth, itemHeight, gap) = Grid;
        bool rtl = IsRightToLeftMode;
        int originX = rtl && _vScroll.Visible ? _vScroll.Width : 0;
        int localX = point.X - originX;

        if (localX < 0 || localX >= ViewportWidth) return -1;

        int col = rtl
            ? (ViewportWidth - localX) / (itemWidth + gap)
            : localX / (itemWidth + gap);
        int row = (point.Y + _scrollOffset) / (itemHeight + gap);

        int index = row * cols + col;
        if (index < 0 || index >= _items.Count) return -1;

        Rectangle card = GetCardRect(index, cols, itemWidth, itemHeight, gap, originX, rtl);
        return card.Contains(point) ? index : -1;
    }

    private Rectangle GetCardRect(
        int index,
        int cols,
        int itemWidth,
        int itemHeight,
        int gap,
        int originX,
        bool rtl)
    {
        int row = index / cols;
        int col = index % cols;
        int x = rtl
            ? originX + ViewportWidth - (col + 1) * (itemWidth + gap) + gap
            : originX + col * (itemWidth + gap);
        int y = row * (itemHeight + gap) - _scrollOffset;

        return new Rectangle(x, y, itemWidth, itemHeight);
    }

    private bool IsInsideAddButton(int index, Point point)
    {
        var (cols, itemWidth, itemHeight, gap) = Grid;
        bool rtl = IsRightToLeftMode;
        int originX = rtl && _vScroll.Visible ? _vScroll.Width : 0;

        Rectangle card = GetCardRect(index, cols, itemWidth, itemHeight, gap, originX, rtl);
        PosCardRects rects = PosCardLayoutCalculator.Calculate(card.Width, card.Height, rtl);
        return Offset(rects.AddButton, card).Contains(point);
    }

    // =========================
    // Theme and lifetime
    // =========================

    private void OnThemeChanged(object? sender, EventArgs e)
    {
        if (IsDisposed) return;

        if (InvokeRequired)
        {
            if (IsHandleCreated)
                BeginInvoke(new Action(ApplyThemeNow));
            return;
        }

        ApplyThemeNow();
    }

    private void ApplyThemeNow()
    {
        if (IsDisposed) return;

        RebuildThemeResources();
        Invalidate();
    }

    private void RebuildThemeResources()
    {
        DisposeThemeResources();

        BackColor = AppColors.Background;
        ForeColor = AppColors.Ink;

        _backgroundBrush = new SolidBrush(AppColors.Background);
        _surfaceBrush = new SolidBrush(AppColors.Surface);
        _hoverBrush = new SolidBrush(AppColors.GridHover);
        _selectionBrush = new SolidBrush(AppColors.GridSelectionBackground);
        _surfaceAltBrush = new SolidBrush(AppColors.SurfaceAlt);
        _primaryBrush = new SolidBrush(AppColors.Primary);
        _primarySoftBrush = new SolidBrush(AppColors.PrimarySoft);
        _dangerBrush = new SolidBrush(AppColors.Danger);
        _warningBrush = new SolidBrush(AppColors.Warning);
        _onDarkBrush = new SolidBrush(AppColors.OnDark);
        _borderPen = new Pen(AppColors.Border);
        _primaryPen = new Pen(AppColors.Primary);
    }

    private void DisposeThemeResources()
    {
        _backgroundBrush.Dispose();
        _surfaceBrush.Dispose();
        _hoverBrush.Dispose();
        _selectionBrush.Dispose();
        _surfaceAltBrush.Dispose();
        _primaryBrush.Dispose();
        _primarySoftBrush.Dispose();
        _dangerBrush.Dispose();
        _warningBrush.Dispose();
        _onDarkBrush.Dispose();
        _borderPen.Dispose();
        _primaryPen.Dispose();
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        UpdateScrollBar();
    }

    protected override void OnRightToLeftChanged(EventArgs e)
    {
        base.OnRightToLeftChanged(e);
        LayoutScrollBar();
        Invalidate();
    }

    protected override void OnGotFocus(EventArgs e)
    {
        base.OnGotFocus(e);
        Invalidate();
    }

    protected override void OnLostFocus(EventArgs e)
    {
        base.OnLostFocus(e);
        Invalidate();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (_themeSubscribed)
            {
                ThemeManager.ThemeChanged -= OnThemeChanged;
                _themeSubscribed = false;
            }

            DisposeThemeResources();

            lock (_imageLock)
            {
                foreach (Image image in _imageCache.Values)
                    image.Dispose();

                _imageCache.Clear();
                _imageLoads.Clear();
                _lru.Clear();
            }
        }

        base.Dispose(disposing);
    }
}
