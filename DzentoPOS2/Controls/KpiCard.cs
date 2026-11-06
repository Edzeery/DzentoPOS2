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
    public class KpiCard : Panel, IThemeAware
    {
        private static readonly Font TitleFont = new("Segoe UI", 9.5F);
        private static readonly Font TrendFont = new("Segoe UI Semibold", 9.5F);
        private static readonly Font ValueFont = new("Segoe UI", 24F, FontStyle.Bold);
        private static readonly Font EstimatedFont = new("Segoe UI", 9F);

        private readonly IconPictureBox _iconBox;
        private readonly Panel _iconBadge;
        private readonly Panel _accentLeft;
        private readonly string _key;
        private readonly int _baseElevation;
        private int _currentElevation;
        private readonly ToolTip _toolTip;

        private KpiValueKind _valueKind = KpiValueKind.Count;
        private int _currencyDigits = 2;
        private decimal _currentNum;
        private decimal _targetNum;
        private System.Windows.Forms.Timer? _animTimer;
        private Color _baseAccent = Color.Gray;
        private Color _appliedAccent = Color.Gray;

        private string _title = string.Empty;
        private string _trendText = string.Empty;
        private KpiTrendKind _trendKind = KpiTrendKind.None;
        private bool _estimated;
        private Rectangle _valueRect;
        private Rectangle _estimatedRect;
        private Rectangle _titleRect;
        private Rectangle _trendRect;

        public static bool ReduceMotion { get; set; }
        static KpiCard()
        {
            ReduceMotion = !SystemInformation.UIEffectsEnabled;
            try { Microsoft.Win32.SystemEvents.UserPreferenceChanged += (_, _) => ReduceMotion = !SystemInformation.UIEffectsEnabled; }
            catch (Exception) { /* ignore */ }
        }

        public static int CurrencyDigits { get; set; } = 2;

        public string CardKey => _key;
        public string CardTitle { get => _title; set { _title = value ?? string.Empty; Invalidate(); } }
        public IconChar Icon { get => _iconBox.IconChar; set => _iconBox.IconChar = value; }
        public Color AccentColor { get => _appliedAccent; set { _baseAccent = value; ApplyAccent(AppColors.AdaptAccent(value)); } }

        public KpiCard() : this("", "", IconChar.QuestionCircle, Color.Gray, 4) { }

        public KpiCard(string key, string title, IconChar icon, Color accent, int baseElevation = 4)
        {
            _key = key;
            _baseElevation = baseElevation;
            _currentElevation = baseElevation;
            _title = title ?? string.Empty;
            _toolTip = new ToolTip
            {
                InitialDelay = 200,
                AutoPopDelay = 8000,
                ReshowDelay = 100,
                ShowAlways = false
            };

            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
            DoubleBuffered = true;

            Size = new Size(300, 150);
            MinimumSize = new Size(200, 120);
            Margin = new Padding(10);
            BackColor = AppColors.Surface;
            Cursor = Cursors.Hand;

            _accentLeft = new Panel { Width = 5, Dock = DockStyle.Left, BackColor = accent };
            _iconBadge = new Panel
            {
                Size = new Size(40, 40),
                Location = new Point(22, 20),
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };
            RadiusControl.Apply(_iconBadge, 10);
            AccentColor = accent;

            _iconBox = new IconPictureBox
            {
                IconChar = icon,
                IconColor = Color.White,
                IconSize = 22,
                Location = new Point(9, 9),
                Size = new Size(22, 22),
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };
            _iconBadge.Controls.Add(_iconBox);

            Controls.AddRange(new Control[] { _accentLeft, _iconBadge });

            MouseEnter += (_, _) => SetHover(true);
            MouseLeave += (_, _) =>
            {
                if (!ClientRectangle.Contains(PointToClient(Cursor.Position)))
                    SetHover(false);
            };
            MouseMove += (_, e) =>
            {
                if (_estimatedRect.Contains(e.Location))
                    _toolTip.Show(AppStrings.EstimatedCostNote, this, e.Location.X + 10, e.Location.Y + 10);
                else if (_titleRect.Contains(e.Location) && TextRenderer.MeasureText(_title, TitleFont).Width > _titleRect.Width)
                    _toolTip.Show(_title, this, e.Location.X + 10, e.Location.Y + 10);
                else
                    _toolTip.Hide(this);
            };

            HandleCreated += (_, _) => ApplyTheme();
            if (IsHandleCreated) ApplyTheme();
            ThemeManager.ThemeChanged += OnThemeChanged;
        }

        private void OnThemeChanged(object? sender, EventArgs e)
        {
            if (IsDisposed) return;
            if (InvokeRequired) { BeginInvoke(ApplyTheme); return; }
            ApplyTheme();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            var rtl = RightToLeft == RightToLeft.Yes;
            var flags = TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine;
            if (rtl) flags |= TextFormatFlags.RightToLeft | TextFormatFlags.Right;

            // background rounded
            using (var path = RoundedRect(ClientRectangle, 12))
            using (var brush = new SolidBrush(AppColors.Surface))
                g.FillPath(brush, path);

            // accent
            _accentLeft.BackColor = _appliedAccent;

            // icon badge already child

            // accent strip side follows reading direction
            _accentLeft.Dock = rtl ? DockStyle.Right : DockStyle.Left;

            // icon badge position follows reading direction
            _iconBadge.Location = rtl
                ? new Point(Width - _iconBadge.Width - 22, 20)
                : new Point(22, 20);

            // trend measure
            int trendWidth = 0;
            if (!string.IsNullOrEmpty(_trendText))
            {
                var trendSize = TextRenderer.MeasureText(g, _trendText, TrendFont, Size, TextFormatFlags.NoPadding);
                trendWidth = trendSize.Width + 8;
            }
            int iconWidth = _iconBadge.Width + 12;

            // title + trend rects (mirrored for RTL, no overlap with icon)
            if (rtl)
            {
                _trendRect = string.IsNullOrEmpty(_trendText)
                    ? Rectangle.Empty
                    : new Rectangle(12, 24, trendWidth, 18);
                int titleX = trendWidth + 24;
                int titleWidth = Math.Max(20, Width - trendWidth - 98);
                _titleRect = new Rectangle(titleX, 24, titleWidth, 18);
            }
            else
            {
                _titleRect = new Rectangle(74, 24, Math.Max(20, Width - trendWidth - 98), 18);
                _trendRect = string.IsNullOrEmpty(_trendText)
                    ? Rectangle.Empty
                    : new Rectangle(Width - trendWidth - 12, 24, trendWidth, 18);
            }

            TextRenderer.DrawText(g, _title, TitleFont, _titleRect, AppColors.InkMuted, flags);

            if (!string.IsNullOrEmpty(_trendText))
                TextRenderer.DrawText(g, _trendText, TrendFont, _trendRect, AppColors.InkMuted, flags);

            // estimated
            if (_estimated)
            {
                _estimatedRect = new Rectangle(rtl ? 12 : Width - 24, 4, 20, 16);
                TextRenderer.DrawText(g, "⚠", EstimatedFont, _estimatedRect, AppColors.Warning, TextFormatFlags.NoPadding);
            }
            else _estimatedRect = Rectangle.Empty;

            // value
            _valueRect = new Rectangle(22, Math.Max(60, Height - 40 - 28), Width - 44, 40);
            var valueText = FormatValue(_currentNum);
            TextRenderer.DrawText(g, valueText, ValueFont, _valueRect, _appliedAccent, TextFormatFlags.NoPadding | TextFormatFlags.SingleLine | (rtl ? TextFormatFlags.RightToLeft : 0));

            // hover effect already background
        }

        private static GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            var path = new GraphicsPath();
            var d = radius * 2;
            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        public void ApplyTheme()
        {
            if (IsDisposed) return;
            BackColor = AppColors.Surface;
            Invalidate();
        }

        private void ApplyAccent(Color color)
        {
            _appliedAccent = color;
            if (_iconBadge != null) _iconBadge.BackColor = ThemeManager.IsDark ? Blend(AppColors.Surface, color, 0.30f) : Blend(Color.White, color, 0.15f);
            if (_iconBox != null) _iconBox.IconColor = Color.White;
            Invalidate();
        }

        private static Color Blend(Color background, Color accent, float accentShare)
            => Color.FromArgb((int)(background.R + (accent.R - background.R) * accentShare), (int)(background.G + (accent.G - background.G) * accentShare), (int)(background.B + (accent.B - background.B) * accentShare));

        public void SetValue(decimal value, KpiValueKind kind, KpiTrendKind trendKind = KpiTrendKind.None, decimal trendPercent = 0, int? currencyDigits = null)
        {
            _valueKind = kind;
            _currencyDigits = currencyDigits ?? CurrencyDigits;
            _targetNum = value;
            ApplyTrend(trendKind, trendPercent);
            if (ReduceMotion || !IsHandleCreated)
            {
                _currentNum = _targetNum;
                Invalidate(_valueRect);
                return;
            }
            _animTimer ??= new System.Windows.Forms.Timer { Interval = 16 };
            _animTimer.Tick -= AnimTick;
            _animTimer.Tick += AnimTick;
            _animTimer.Start();
        }

        private void AnimTick(object? sender, EventArgs e)
        {
            if (_currentNum == _targetNum) { _animTimer?.Stop(); return; }
            var diff = _targetNum - _currentNum;
            _currentNum += diff / 8m;
            if (Math.Abs(_targetNum - _currentNum) < 0.0001m) _currentNum = _targetNum;
            Invalidate(_valueRect);
        }

        private void ApplyTrend(KpiTrendKind kind, decimal percent)
        {
            _trendKind = kind;
            _trendText = kind switch
            {
                KpiTrendKind.Change => percent == 0 ? "0%" : $"{(percent >= 0 ? "\u2191" : "\u2193")} {Math.Abs(percent):0.#}%",
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
            _estimated = value;
            Invalidate();
        }

        public void SetTooltip(string text) { _toolTip.SetToolTip(this, text); }

        public void SetDetail(string text) { /* no-op in owner-draw */ }

        private void SetHover(bool hover)
        {
            BackColor = hover ? Blend(AppColors.Surface, _appliedAccent, 0.05f) : AppColors.Surface;
            Invalidate();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ThemeManager.ThemeChanged -= OnThemeChanged;
                _animTimer?.Dispose();
                _toolTip?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
