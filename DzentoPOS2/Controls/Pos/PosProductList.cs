using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;
using DzentoPOS2.App.Services.Sales;
using DzentoPOS2.Core.Configuration;
using DzentoPOS2.Shared.Helpers;
using DzentoPOS2.WinForms.Themes;

namespace DzentoPOS2.WinForms.Controls.Pos;

/// <summary>
/// Virtualized, owner-drawn product list for the POS screen.
/// Draws only visible items; handles RTL, dark mode, card/list modes.
/// </summary>
public class PosProductList : Control
{
    private IReadOnlyList<PosProductItem> _items = Array.Empty<PosProductItem>();
    private PosViewMode _mode = PosViewMode.Cards;
    private int _selectedIndex = -1;
    private int _scrollOffset = 0;
    private VScrollBar _vScroll = new() { Dock = DockStyle.Right, Width = 16 };
    private readonly System.Windows.Forms.Timer _animTimer;
    private int _hoverIndex = -1;
    private readonly Dictionary<string, Image> _imageCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly LinkedList<string> _lru = new();
    private const int ImageCacheCapacity = 64;
    private static readonly string ImagesRoot = Path.Combine(AppPaths.Root, "images");
    private readonly object _imgLock = new();

    public event EventHandler<PosProductItem>? ItemClicked;
    public event EventHandler<PosProductItem>? ItemDoubleClicked;

    public PosProductList()
    {
        SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
        TabStop = true;

        _vScroll.Scroll += (_, _) => { _scrollOffset = _vScroll.Value; Invalidate(); };
        Controls.Add(_vScroll);

        _animTimer = new System.Windows.Forms.Timer { Interval = AppStyle.AnimationInterval };
        _animTimer.Tick += (_, _) => Invalidate();
        _animTimer.Start();

        ThemeManager.ThemeChanged += OnThemeChanged;

        MouseMove += OnMouseMove;
        MouseLeave += (_, _) => { _hoverIndex = -1; Invalidate(); };
        MouseClick += OnMouseClick;
        MouseDoubleClick += OnMouseDoubleClick;
        KeyDown += OnKeyDown;
        Resize += (_, _) => UpdateScrollBar();
    }

    public IReadOnlyList<PosProductItem> Items
    {
        get => _items;
        set { _items = value ?? Array.Empty<PosProductItem>(); _scrollOffset = 0; UpdateScrollBar(); Invalidate(); }
    }

    public PosViewMode ViewMode
    {
        get => _mode;
        set { _mode = value; UpdateScrollBar(); Invalidate(); }
    }

    public int ScrollOffset
    {
        get => _scrollOffset;
        set { _scrollOffset = Math.Max(0, value); _vScroll.Value = Math.Min(_vScroll.Maximum, _scrollOffset); Invalidate(); }
    }

    public bool IsRightToLeftMode => RightToLeft == RightToLeft.Yes;

    private void OnThemeChanged(object? sender, EventArgs e) => Invalidate();

    private int ItemHeight => PosCardLayoutCalculator.GetGrid(Math.Max(1, ClientSize.Width - _vScroll.Width), _mode).ItemHeight;
    private int Columns => PosCardLayoutCalculator.GetGrid(Math.Max(1, ClientSize.Width - _vScroll.Width), _mode).Columns;
    private int Gap => PosCardLayoutCalculator.GetGrid(Math.Max(1, ClientSize.Width - _vScroll.Width), _mode).Gap;

    private int TotalHeight
    {
        get
        {
            var (cols, _, h, gap) = PosCardLayoutCalculator.GetGrid(Math.Max(1, ClientSize.Width - _vScroll.Width), _mode);
            int rows = (_items.Count + cols - 1) / cols;
            return rows * (h + gap);
        }
    }

    private void UpdateScrollBar()
    {
        int availH = ClientSize.Height;
        int total = TotalHeight;
        _vScroll.Maximum = Math.Max(0, total - availH);
        _vScroll.LargeChange = Math.Max(1, availH);
        _vScroll.SmallChange = ItemHeight;
        _vScroll.Visible = total > availH;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        int availW = ClientSize.Width - (_vScroll.Visible ? _vScroll.Width : 0);
        var (cols, itemW, itemH, gap) = PosCardLayoutCalculator.GetGrid(Math.Max(1, availW), _mode);
        bool rtl = IsRightToLeftMode;

        int scroll = _scrollOffset;
        int firstRow = scroll / (itemH + gap);
        int visibleRows = (ClientSize.Height / (itemH + gap)) + 2;
        int firstIdx = firstRow * cols;
        int lastIdx = Math.Min(_items.Count, firstIdx + visibleRows * cols);

        // Background
        using var bgBrush = new SolidBrush(AppColors.Background);
        g.FillRectangle(bgBrush, ClientRectangle);

        for (int i = firstIdx; i < lastIdx; i++)
        {
            int row = i / cols;
            int col = i % cols;
            int x = rtl ? availW - (col + 1) * (itemW + gap) + gap : col * (itemW + gap);
            int y = row * (itemH + gap) - scroll;
            var cardRect = new Rectangle(x, y, itemW, itemH);

            if (cardRect.Bottom < 0 || cardRect.Top > ClientSize.Height) continue;

            DrawItem(g, _items[i], cardRect, i == _selectedIndex, i == _hoverIndex, rtl);
        }

        // Empty state
        if (_items.Count == 0)
        {
            using var font = new Font("Segoe UI", 12F);
            using var brush = new SolidBrush(AppColors.InkMuted);
            string text = "No products";
            var size = g.MeasureString(text, font);
            g.DrawString(text, font, brush, (ClientSize.Width - size.Width) / 2, (ClientSize.Height - size.Height) / 2);
        }
    }

    private void DrawItem(Graphics g, PosProductItem item, Rectangle cardRect, bool selected, bool hovered, bool rtl)
    {
        // Card background
        Color cardBg = selected ? AppColors.GridSelectionBackground
                      : hovered ? AppColors.GridHover
                      : AppColors.Surface;
        using var cardBrush = new SolidBrush(cardBg);
        using var cardPath = RoundedRect(cardRect, 8);
        g.FillPath(cardBrush, cardPath);

        // Border
        using var borderPen = new Pen(AppColors.Border, 1);
        g.DrawPath(borderPen, cardPath);

        var rects = PosCardLayoutCalculator.Calculate(cardRect.Width, cardRect.Height, rtl);
        // Offset rects by card position
        rects = rects with
        {
            Image = new Rectangle(cardRect.X + rects.Image.X, cardRect.Y + rects.Image.Y, rects.Image.Width, rects.Image.Height),
            Name = new Rectangle(cardRect.X + rects.Name.X, cardRect.Y + rects.Name.Y, rects.Name.Width, rects.Name.Height),
            Sku = new Rectangle(cardRect.X + rects.Sku.X, cardRect.Y + rects.Sku.Y, rects.Sku.Width, rects.Sku.Height),
            Price = new Rectangle(cardRect.X + rects.Price.X, cardRect.Y + rects.Price.Y, rects.Price.Width, rects.Price.Height),
            Badge = new Rectangle(cardRect.X + rects.Badge.X, cardRect.Y + rects.Badge.Y, rects.Badge.Width, rects.Badge.Height),
            AddButton = new Rectangle(cardRect.X + rects.AddButton.X, cardRect.Y + rects.AddButton.Y, rects.AddButton.Width, rects.AddButton.Height)
        };

        // Image
        var img = GetCachedImage(item.ThumbnailPath ?? item.ImagePath);
        if (img != null)
        {
            g.DrawImage(img, rects.Image);
        }
        else
        {
            using var placeholderBrush = new SolidBrush(AppColors.SurfaceAlt);
            g.FillRectangle(placeholderBrush, rects.Image);

            // Unified fallback: the product's initial, never an empty box.
            var initial = string.IsNullOrWhiteSpace(item.Name)
                ? "?"
                : item.Name.Trim()[0].ToString();
            using var initialFont = new Font(
                "Segoe UI",
                Math.Max(10F, rects.Image.Height / 4F),
                FontStyle.Bold);
            using var initialBrush = new SolidBrush(AppColors.InkMuted);
            var initialFormat = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            g.DrawString(initial, initialFont, initialBrush, rects.Image, initialFormat);
        }

        // Name
        using var nameFont = new Font("Segoe UI", 10F, FontStyle.Bold);
        using var nameBrush = new SolidBrush(AppColors.Ink);
        var nameFormat = new StringFormat
        {
            Alignment = rtl ? StringAlignment.Far : StringAlignment.Near,
            LineAlignment = StringAlignment.Near,
            Trimming = StringTrimming.EllipsisCharacter,
            FormatFlags = StringFormatFlags.LineLimit
        };
        g.DrawString(item.Name, nameFont, nameBrush, rects.Name, nameFormat);

        // SKU
        if (!string.IsNullOrWhiteSpace(item.SKU))
        {
            using var skuFont = new Font("Segoe UI", 8F);
            using var skuBrush = new SolidBrush(AppColors.InkMuted);
            var skuFormat = new StringFormat
            {
                Alignment = rtl ? StringAlignment.Far : StringAlignment.Near,
                LineAlignment = StringAlignment.Near,
                Trimming = StringTrimming.EllipsisCharacter
            };
            g.DrawString(item.SKU, skuFont, skuBrush, rects.Sku, skuFormat);
        }

        // Price
        using var priceFont = new Font("Segoe UI", 10F, FontStyle.Bold);
        using var priceBrush = new SolidBrush(AppColors.Primary);
        var priceFormat = new StringFormat
        {
            Alignment = rtl ? StringAlignment.Near : StringAlignment.Far,
            LineAlignment = StringAlignment.Center
        };
        g.DrawString(
            item.SalePrice.ToString("N2", KpiValueFormatter.NormalizeCulture(CultureInfo.CurrentCulture)),
            priceFont,
            priceBrush,
            rects.Price,
            priceFormat);

        // Stock badge
        if (item.Stock <= 0)
        {
            using var badgeBrush = new SolidBrush(AppColors.Danger);
            using var badgeFont = new Font("Segoe UI", 8F, FontStyle.Bold);
            using var badgeTextBrush = new SolidBrush(AppColors.OnDark);
            g.FillRectangle(badgeBrush, rects.Badge);
            var badgeFormat = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            g.DrawString("Out", badgeFont, badgeTextBrush, rects.Badge, badgeFormat);
        }
        else if (item.Stock < 5)
        {
            using var badgeBrush = new SolidBrush(AppColors.Warning);
            using var badgeFont = new Font("Segoe UI", 8F, FontStyle.Bold);
            using var badgeTextBrush = new SolidBrush(Color.FromArgb(48, 33, 0));
            g.FillRectangle(badgeBrush, rects.Badge);
            var badgeFormat = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            g.DrawString(item.Stock.ToString("0"), badgeFont, badgeTextBrush, rects.Badge, badgeFormat);
        }

        // Add button
        using var btnBrush = new SolidBrush(AppColors.Primary);
        using var btnPath = RoundedRect(rects.AddButton, 4);
        g.FillPath(btnBrush, btnPath);
        using var btnFont = new Font("Segoe UI", 12F, FontStyle.Bold);
        using var btnTextBrush = new SolidBrush(AppColors.OnDark);
        var btnFormat = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        g.DrawString("+", btnFont, btnTextBrush, rects.AddButton, btnFormat);
    }

    private Image? GetCachedImage(string? path)
    {
        var full = PosImagePath.Resolve(ImagesRoot, path);
        if (full is null) return null;

        lock (_imgLock)
        {
            if (_imageCache.TryGetValue(full, out var cached))
            {
                // Move to front of LRU
                if (_lru.Contains(full))
                {
                    _lru.Remove(full);
                    _lru.AddFirst(full);
                }
                return new Bitmap(cached);
            }
        }

        // Load async (fire and forget for now, return null until loaded)
        _ = LoadImageAsync(full);
        return null;
    }

    private async System.Threading.Tasks.Task LoadImageAsync(string full)
    {
        try
        {
            // Use a simple file load for now
            await System.Threading.Tasks.Task.Run(() =>
            {
                if (!System.IO.File.Exists(full)) return;
                using var fs = new System.IO.FileStream(full, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                using var loaded = Image.FromStream(fs);
                var copy = new Bitmap(loaded);
                lock (_imgLock)
                {
                    if (_imageCache.Count >= ImageCacheCapacity)
                    {
                        var last = _lru.Last;
                        if (last != null)
                        {
                            _lru.RemoveLast();
                            if (_imageCache.TryGetValue(last.Value, out var old))
                            {
                                old.Dispose();
                                _imageCache.Remove(last.Value);
                            }
                        }
                    }
                    var node = _lru.AddFirst(full);
                    _imageCache[full] = copy;
                }
                Invalidate();
            });
        }
        catch (Exception) { /* best effort image load */ }
    }

    private void OnMouseMove(object? sender, MouseEventArgs e)
    {
        int idx = HitTest(e.Location);
        if (idx != _hoverIndex)
        {
            _hoverIndex = idx;
            Invalidate();
        }
    }

    private void OnMouseClick(object? sender, MouseEventArgs e)
    {
        int idx = HitTest(e.Location);
        if (idx >= 0 && idx < _items.Count)
        {
            _selectedIndex = idx;
            ItemClicked?.Invoke(this, _items[idx]);
            Invalidate();
        }
    }

    private void OnMouseDoubleClick(object? sender, MouseEventArgs e)
    {
        int idx = HitTest(e.Location);
        if (idx >= 0 && idx < _items.Count)
        {
            ItemDoubleClicked?.Invoke(this, _items[idx]);
        }
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (_items.Count == 0) return;
        int cols = Columns;
        switch (e.KeyCode)
        {
            case Keys.Down:
                _selectedIndex = Math.Min(_items.Count - 1, _selectedIndex + cols);
                EnsureVisible(_selectedIndex);
                Invalidate();
                e.Handled = true;
                break;
            case Keys.Up:
                _selectedIndex = Math.Max(0, _selectedIndex - cols);
                EnsureVisible(_selectedIndex);
                Invalidate();
                e.Handled = true;
                break;
            case Keys.Enter:
                if (_selectedIndex >= 0 && _selectedIndex < _items.Count)
                    ItemClicked?.Invoke(this, _items[_selectedIndex]);
                e.Handled = true;
                break;
        }
    }

    private int HitTest(Point pt)
    {
        int availW = ClientSize.Width - (_vScroll.Visible ? _vScroll.Width : 0);
        var (cols, itemW, itemH, gap) = PosCardLayoutCalculator.GetGrid(Math.Max(1, availW), _mode);
        bool rtl = IsRightToLeftMode;
        int scroll = _scrollOffset;

        int col = rtl ? (availW - pt.X) / (itemW + gap) : pt.X / (itemW + gap);
        int row = (pt.Y + scroll) / (itemH + gap);
        int idx = row * cols + col;

        if (idx < 0 || idx >= _items.Count) return -1;

        int x = rtl ? availW - (col + 1) * (itemW + gap) + gap : col * (itemW + gap);
        int y = row * (itemH + gap) - scroll;
        var rect = new Rectangle(x, y, itemW, itemH);
        return rect.Contains(pt) ? idx : -1;
    }

    private void EnsureVisible(int idx)
    {
        if (idx < 0) return;
        int cols = Columns;
        int itemH = ItemHeight;
        int gap = Gap;
        int row = idx / cols;
        int top = row * (itemH + gap);
        int bottom = top + itemH;
        if (top < _scrollOffset) _scrollOffset = top;
        else if (bottom > _scrollOffset + ClientSize.Height) _scrollOffset = bottom - ClientSize.Height;
        _vScroll.Value = Math.Max(0, Math.Min(_vScroll.Maximum, _scrollOffset));
    }

    private static GraphicsPath RoundedRect(Rectangle r, int radius)
    {
        var path = new GraphicsPath();
        int d = radius * 2;
        path.AddArc(r.X, r.Y, d, d, 180, 90);
        path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
        path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
        path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _animTimer.Stop();
            _animTimer.Dispose();
            ThemeManager.ThemeChanged -= OnThemeChanged;
            lock (_imgLock)
            {
                foreach (var img in _imageCache.Values) img.Dispose();
                _imageCache.Clear();
                _lru.Clear();
            }
        }
        base.Dispose(disposing);
    }
}
