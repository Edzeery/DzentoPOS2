using FontAwesome.Sharp;
using DzentoPOS2.Localization.Resources;
using DzentoPOS2.Shared.Contracts.DTOs.Reports;
using DzentoPOS2.Shared.Helpers;
using DzentoPOS2.WinForms.ServicesUI;
using DzentoPOS2.WinForms.Themes;
using System.Globalization;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace DzentoPOS2.WinForms.Controls
{
    /// <summary>
    /// Owner-drawn KPI card. Everything (surface, accent, icon badge, title,
    /// trend, warning, value and caption) is painted in a single
    /// <see cref="OnPaint"/> pass; there are no child controls and no absolute
    /// coordinates outside <see cref="KpiCardLayout.ComputeLayout"/>.
    /// </summary>
    public class KpiCard : Panel, IThemeAware
    {
        private const int CardRadius = 12;
        private const int BadgeRadius = 10;
        private const int AccentWidth = 5;
        private const int Padding = 16;

        private const float TitlePoints = 9.5f;
        private const float TrendPoints = 9.5f;
        private const float ValuePoints = 24f;
        private const float CaptionPoints = 9f;
        private const float WarningPoints = 11f;
        private const float MinValuePoints = 13f;

        // FontAwesome loads its glyphs into a private font collection. The probe is a
        // tiny subclass because the family resolver (FontFor) is protected; it is
        // kept alive for the process lifetime and never touched from OnPaint.
        private static readonly GlyphFontProbe GlyphProbe = new();

        private sealed class GlyphFontProbe : IconPictureBox
        {
            public FontFamily FamilyFor(IconChar icon) => FontFor(icon);
        }

        // GDI+ is required for the private FontAwesome family; TextRenderer (GDI)
        // renders it as a missing-glyph box.
        private static readonly StringFormat GlyphFormat = CreateGlyphFormat();

        private static StringFormat CreateGlyphFormat()
            => new(StringFormat.GenericTypographic)
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center,
                FormatFlags = StringFormatFlags.NoClip | StringFormatFlags.NoWrap
            };

        private readonly KpiPaintCache _paint = new();
        private readonly ToolTip _toolTip;
        private readonly string _key;

        private IconChar _icon;
        private FontFamily _glyphFamily = FontFamily.GenericSansSerif;
        private Color _baseAccent = Color.Gray;
        private Color _appliedAccent = Color.Gray;

        private KpiValueKind _valueKind = KpiValueKind.Count;
        private int _currencyDigits = 2;
        private decimal _currentNum;
        private decimal _targetNum;
        private System.Windows.Forms.Timer? _animTimer;

        private string _title = string.Empty;
        private string _caption = string.Empty;
        private string _trendText = string.Empty;
        private decimal _trendPercent;
        private KpiTrendKind _trendKind = KpiTrendKind.None;
        private bool _estimated;
        private bool _hover;

        private Rectangle _badgeBounds;
        private GraphicsPath? _badgePath;
        private Size _cardPathBounds;
        private GraphicsPath? _cardPath;
        private KpiCardLayoutResult _layout;

        public static bool ReduceMotion { get; set; }

        static KpiCard()
        {
            ReduceMotion = !SystemInformation.UIEffectsEnabled;
            try
            {
                Microsoft.Win32.SystemEvents.UserPreferenceChanged +=
                    (_, _) => ReduceMotion = !SystemInformation.UIEffectsEnabled;
            }
            catch (Exception)
            {
                // The system event is a best-effort refinement only.
            }
        }

        public static int CurrencyDigits { get; set; } = 2;

        public string CardKey => _key;

        public string CardTitle
        {
            get => _title;
            set
            {
                _title = value ?? string.Empty;
                Invalidate();
            }
        }

        public IconChar Icon
        {
            get => _icon;
            set
            {
                if (_icon == value) return;
                _icon = value;
                _glyphFamily = GlyphProbe.FamilyFor(value);
                Invalidate();
            }
        }

        public Color AccentColor
        {
            get => _appliedAccent;
            set
            {
                _baseAccent = value;
                ApplyAccent(AppColors.AdaptAccent(value));
            }
        }

        public KpiCard() : this("", "", IconChar.QuestionCircle, Color.Gray) { }

        public KpiCard(string key, string title, IconChar icon, Color accent)
        {
            _key = key ?? string.Empty;
            _title = title ?? string.Empty;
            _icon = icon;
            _glyphFamily = GlyphProbe.FamilyFor(icon);

            _toolTip = new ToolTip
            {
                InitialDelay = 200,
                AutoPopDelay = 8000,
                ReshowDelay = 100,
                ShowAlways = false
            };

            SetStyle(
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.ResizeRedraw,
                true);
            DoubleBuffered = true;

            Size = new Size(300, 150);
            MinimumSize = new Size(200, 120);
            Margin = new Padding(10);
            BackColor = AppColors.Surface;
            Cursor = Cursors.Hand;

            AccentColor = accent;

            MouseEnter += (_, _) => SetHover(true);
            MouseLeave += (_, _) => SetHover(false);
            MouseMove += OnMouseMove;

            HandleCreated += (_, _) => ApplyTheme();
            ThemeManager.ThemeChanged += OnThemeChanged;
        }

        /// <summary>
        /// Pure geometry used by painting, hit testing and tooltips. The measured
        /// trend width and the current DPI-scaled row heights are passed in.
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var graphics = e.Graphics;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            _paint.EnsureDpi(DeviceDpi);

            bool rtl = RightToLeft == RightToLeft.Yes;
            _layout = KpiCardLayout.ComputeLayout(ClientSize, rtl, BuildMetrics(graphics));

            DrawSurface(graphics);
            DrawAccent(graphics, rtl);
            DrawIcon(graphics, _layout.Icon);
            DrawTitle(graphics, _layout.Title, rtl);
            DrawTrend(graphics, _layout.Trend, rtl);
            DrawWarning(graphics, _layout.Estimated);
            DrawValue(graphics, _layout.Value, rtl);
            DrawCaption(graphics, _layout.Caption, rtl);
        }

        private KpiCardMetrics BuildMetrics(Graphics graphics)
        {
            int trendWidth = 0;
            if (!string.IsNullOrEmpty(_trendText))
            {
                var text = RightToLeft == RightToLeft.Yes ? "\u200E" + _trendText : _trendText;
                var font = _paint.GetTextFont("Segoe UI Semibold", TrendPoints, FontStyle.Regular);
                var measured = TextRenderer.MeasureText(
                    graphics,
                    text,
                    font,
                    new Size(100_000, 1_000),
                    TextFormatFlags.NoPadding | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix);
                trendWidth = measured.Width + 4;
            }

            int titleWidth = 0;
            if (!string.IsNullOrEmpty(_title))
            {
                var measured = TextRenderer.MeasureText(
                    graphics,
                    _title,
                    _paint.GetTextFont("Segoe UI", TitlePoints, FontStyle.Regular),
                    new Size(100_000, 1_000),
                    TextFormatFlags.NoPadding | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix);
                titleWidth = measured.Width;
            }

            return KpiCardMetrics.Default with
            {
                AccentWidth = AccentWidth,
                Padding = Padding,
                HasTrend = trendWidth > 0,
                TrendWidth = trendWidth,
                TitleWidth = titleWidth,
                HasEstimated = _estimated,
                HasCaption = !string.IsNullOrEmpty(_caption)
            };
        }

        private void DrawSurface(Graphics graphics)
        {
            var path = GetCardPath();
            var color = _hover
                ? Blend(AppColors.Surface, _appliedAccent, 0.05f)
                : AppColors.Surface;

            graphics.FillPath(_paint.GetBrush(color), path);
        }

        private void DrawAccent(Graphics graphics, bool rtl)
        {
            if (ClientSize.Width <= 0 || ClientSize.Height <= 0) return;

            var path = GetCardPath();
            var state = graphics.Save();
            try
            {
                graphics.SetClip(path);
                var x = rtl ? ClientSize.Width - AccentWidth : 0;
                graphics.FillRectangle(
                    _paint.GetBrush(_appliedAccent),
                    new Rectangle(x, 0, AccentWidth, ClientSize.Height));
            }
            finally
            {
                graphics.Restore(state);
            }
        }

        private void DrawIcon(Graphics graphics, Rectangle bounds)
        {
            if (bounds.Width <= 2 || bounds.Height <= 2) return;

            var background = ThemeManager.IsDark
                ? Blend(AppColors.Surface, _appliedAccent, 0.32f)
                : Blend(Color.White, _appliedAccent, 0.14f);

            graphics.FillPath(_paint.GetBrush(background), GetBadgePath(bounds));

            if (_icon == IconChar.None) return;

            var glyph = char.ConvertFromUtf32((int)_icon);
            if (string.IsNullOrEmpty(glyph)) return;

            var foreground = ThemeManager.IsDark
                ? Color.White
                : Blend(_appliedAccent, Color.Black, 0.35f);

            var font = _paint.GetPixelFont(_glyphFamily, bounds.Height * 0.62f, FontStyle.Regular);
            graphics.DrawString(
                glyph,
                font,
                _paint.GetBrush(foreground),
                new RectangleF(bounds.X, bounds.Y, bounds.Width, bounds.Height),
                GlyphFormat);
        }

        private void DrawTitle(Graphics graphics, Rectangle bounds, bool rtl)
        {
            if (bounds.Width <= 0 || string.IsNullOrEmpty(_title)) return;

            var flags = TextFormatFlags.EndEllipsis |
                        TextFormatFlags.NoPadding |
                        TextFormatFlags.SingleLine |
                        TextFormatFlags.NoPrefix |
                        TextFormatFlags.VerticalCenter |
                        (rtl
                            ? TextFormatFlags.Right | TextFormatFlags.RightToLeft
                            : TextFormatFlags.Left);

            TextRenderer.DrawText(
                graphics,
                _title,
                _paint.GetTextFont("Segoe UI", TitlePoints, FontStyle.Regular),
                bounds,
                AppColors.InkMuted,
                flags);
        }

        private void DrawTrend(Graphics graphics, Rectangle bounds, bool rtl)
        {
            if (bounds.Width <= 0 || string.IsNullOrEmpty(_trendText)) return;

            var text = rtl ? "\u200E" + _trendText : _trendText;
            var color = _trendKind switch
            {
                KpiTrendKind.Change when _trendPercent >= 0 => AppColors.Success,
                KpiTrendKind.Change => AppColors.Danger,
                _ => AppColors.InkMuted
            };

            var flags = TextFormatFlags.EndEllipsis |
                        TextFormatFlags.NoPadding |
                        TextFormatFlags.SingleLine |
                        TextFormatFlags.NoPrefix |
                        TextFormatFlags.VerticalCenter |
                        (rtl ? TextFormatFlags.Right : TextFormatFlags.Left);

            if (rtl && _trendKind == KpiTrendKind.Margin)
                flags |= TextFormatFlags.RightToLeft;

            TextRenderer.DrawText(
                graphics,
                text,
                _paint.GetTextFont("Segoe UI Semibold", TrendPoints, FontStyle.Regular),
                bounds,
                color,
                flags);
        }

        private void DrawWarning(Graphics graphics, Rectangle bounds)
        {
            if (bounds.Width <= 0) return;

            TextRenderer.DrawText(
                graphics,
                "\u26A0",
                _paint.GetTextFont("Segoe UI", WarningPoints, FontStyle.Regular),
                bounds,
                AppColors.Warning,
                TextFormatFlags.HorizontalCenter |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.NoPadding |
                TextFormatFlags.SingleLine |
                TextFormatFlags.NoPrefix);
        }

        private void DrawValue(Graphics graphics, Rectangle bounds, bool rtl)
        {
            if (bounds.Width <= 0) return;

            var text = FormatValue(_currentNum);
            if (rtl) text = "\u200E" + text;

            var font = FitValueFont(graphics, text, bounds.Width);
            var flags = TextFormatFlags.NoPadding |
                        TextFormatFlags.SingleLine |
                        TextFormatFlags.NoPrefix |
                        TextFormatFlags.VerticalCenter |
                        (rtl ? TextFormatFlags.Right : TextFormatFlags.Left);

            // The value has the strongest contrast in the card (Ink, not accent).
            TextRenderer.DrawText(graphics, text, font, bounds, AppColors.Ink, flags);
        }

        private void DrawCaption(Graphics graphics, Rectangle bounds, bool rtl)
        {
            if (bounds.Width <= 0 || string.IsNullOrEmpty(_caption)) return;

            var flags = TextFormatFlags.EndEllipsis |
                        TextFormatFlags.NoPadding |
                        TextFormatFlags.SingleLine |
                        TextFormatFlags.NoPrefix |
                        TextFormatFlags.VerticalCenter |
                        (rtl
                            ? TextFormatFlags.Right | TextFormatFlags.RightToLeft
                            : TextFormatFlags.Left);

            TextRenderer.DrawText(
                graphics,
                _caption,
                _paint.GetTextFont("Segoe UI", CaptionPoints, FontStyle.Regular),
                bounds,
                AppColors.InkMuted,
                flags);
        }

        private Font FitValueFont(Graphics graphics, string text, int maxWidth)
        {
            var size = ValuePoints;
            while (size > MinValuePoints)
            {
                var font = _paint.GetTextFont("Segoe UI", size, FontStyle.Bold);
                var measured = TextRenderer.MeasureText(
                    graphics,
                    text,
                    font,
                    new Size(100_000, 1_000),
                    TextFormatFlags.NoPadding | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix);

                if (measured.Width <= maxWidth) return font;
                size -= 1f;
            }

            return _paint.GetTextFont("Segoe UI", size, FontStyle.Bold);
        }

        private GraphicsPath GetCardPath()
        {
            if (_cardPath != null && _cardPathBounds == ClientSize)
                return _cardPath;

            _cardPath?.Dispose();
            _cardPathBounds = ClientSize;
            _cardPath = CreateRoundedPath(new Rectangle(Point.Empty, ClientSize), CardRadius);
            return _cardPath;
        }

        private GraphicsPath GetBadgePath(Rectangle bounds)
        {
            if (_badgePath != null && _badgeBounds == bounds)
                return _badgePath;

            _badgePath?.Dispose();
            _badgeBounds = bounds;
            _badgePath = CreateRoundedPath(bounds, BadgeRadius);
            return _badgePath;
        }

        private static GraphicsPath CreateRoundedPath(Rectangle bounds, int radius)
        {
            var path = new GraphicsPath();

            if (bounds.Width <= 0 || bounds.Height <= 0)
                return path;

            int diameter = Math.Min(radius * 2, Math.Min(bounds.Width, bounds.Height));
            if (diameter <= 0)
            {
                path.AddRectangle(bounds);
                return path;
            }

            path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void OnMouseMove(object? sender, MouseEventArgs e)
        {
            if (_layout.Estimated.Width > 0 && _layout.Estimated.Contains(e.Location))
            {
                _toolTip.Show(AppStrings.EstimatedCostNote, this, e.Location.X + 10, e.Location.Y + 10);
                return;
            }

            if (_layout.Title.Width > 0 &&
                _layout.Title.Contains(e.Location) &&
                TextRenderer.MeasureText(_title, _paint.GetTextFont("Segoe UI", TitlePoints, FontStyle.Regular)).Width > _layout.Title.Width)
            {
                _toolTip.Show(_title, this, e.Location.X + 10, e.Location.Y + 10);
                return;
            }

            _toolTip.Hide(this);
        }

        public void ApplyTheme()
        {
            if (IsDisposed) return;

            BackColor = AppColors.Surface;
            _appliedAccent = AppColors.AdaptAccent(_baseAccent);
            _paint.ClearBrushes();
            Invalidate();
        }

        private void ApplyAccent(Color color)
        {
            _appliedAccent = color;
            Invalidate();
        }

        private static Color Blend(Color background, Color accent, float accentShare)
            => Color.FromArgb(
                (int)(background.R + (accent.R - background.R) * accentShare),
                (int)(background.G + (accent.G - background.G) * accentShare),
                (int)(background.B + (accent.B - background.B) * accentShare));

        public void SetValue(
            decimal value,
            KpiValueKind kind,
            KpiTrendKind trendKind = KpiTrendKind.None,
            decimal trendPercent = 0,
            int? currencyDigits = null)
        {
            _valueKind = kind;
            _currencyDigits = currencyDigits ?? CurrencyDigits;
            _targetNum = value;
            ApplyTrend(trendKind, trendPercent);

            if (ReduceMotion || !IsHandleCreated)
            {
                _currentNum = _targetNum;
                Invalidate();
                return;
            }

            _animTimer ??= new System.Windows.Forms.Timer { Interval = 16 };
            _animTimer.Tick -= AnimTick;
            _animTimer.Tick += AnimTick;
            _animTimer.Start();
        }

        private void AnimTick(object? sender, EventArgs e)
        {
            if (_currentNum == _targetNum)
            {
                _animTimer?.Stop();
                return;
            }

            var diff = _targetNum - _currentNum;
            _currentNum += diff / 8m;
            if (Math.Abs(_targetNum - _currentNum) < 0.0001m)
                _currentNum = _targetNum;

            if (_layout.Value.Width > 0)
                Invalidate(_layout.Value);
            else
                Invalidate();
        }

        private void ApplyTrend(KpiTrendKind kind, decimal percent)
        {
            _trendKind = kind;
            _trendPercent = percent;
            _trendText = kind switch
            {
                KpiTrendKind.Change => percent == 0
                    ? "0%"
                    : $"{(percent >= 0 ? "\u2191" : "\u2193")} {Math.Abs(percent):0.#}%",
                KpiTrendKind.Margin => $"{AppStrings.Margin} {percent:0.#}%",
                _ => string.Empty
            };

            Invalidate();
        }

        private string FormatValue(decimal value)
        {
            var culture = CultureInfo.CurrentCulture;
            return KpiValueFormatter.Format(value, _valueKind, culture, _currencyDigits);
        }

        public void SetEstimated(bool value)
        {
            if (_estimated == value) return;
            _estimated = value;
            Invalidate();
        }

        public void SetTooltip(string text) => _toolTip.SetToolTip(this, text);

        /// <summary>Optional caption line under the value (e.g. an estimated note).</summary>
        public void SetDetail(string text)
        {
            _caption = text ?? string.Empty;
            Invalidate();
        }

        private void SetHover(bool hover)
        {
            if (_hover == hover) return;
            _hover = hover;
            Cursor = Cursors.Hand;
            Invalidate();
        }

        private void OnThemeChanged(object? sender, EventArgs e)
        {
            if (IsDisposed) return;
            if (InvokeRequired)
            {
                BeginInvoke(ApplyTheme);
                return;
            }

            ApplyTheme();
        }

        protected override void OnDpiChangedAfterParent(EventArgs e)
        {
            base.OnDpiChangedAfterParent(e);
            _paint.Reset();
            Invalidate();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ThemeManager.ThemeChanged -= OnThemeChanged;
                _animTimer?.Dispose();
                _toolTip.Dispose();
                _paint.Dispose();
                _cardPath?.Dispose();
                _badgePath?.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}