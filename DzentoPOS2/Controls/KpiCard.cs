using FontAwesome.Sharp;
using DzentoPOS2.Localization.Resources;
using DzentoPOS2.Shared.Contracts.DTOs.Reports;
using DzentoPOS2.Shared.Helpers;
using DzentoPOS2.WinForms.ServicesUI;
using DzentoPOS2.WinForms.Themes;
using System.Globalization;

namespace DzentoPOS2.WinForms.Controls
{
    /// <summary>
    /// A dashboard KPI tile. It only ever carries a numeric value + a kind; it
    /// never parses a formatted string back into a number. The animation
    /// interpolates the number and formats each frame through
    /// <see cref="KpiValueFormatter"/>, so the displayed text can never become
    /// "44,44,891".
    /// </summary>
    public class KpiCard : Panel, IThemeAware
    {
        private readonly Label _lblTitle;
        private readonly Label _lblValue;
        private readonly Label _lblTrend;
        private readonly IconPictureBox _iconBox;
        private readonly Panel _iconBadge;
        private readonly Panel _accentLeft;
        private readonly string _key;
        private readonly int _baseElevation;
        private int _currentElevation;

        private KpiValueKind _valueKind = KpiValueKind.Count;
        private int _currencyDigits = 2;
        private decimal _currentNum;
        private decimal _targetNum;
        private System.Windows.Forms.Timer? _animTimer;
        private Color _baseAccent = Color.Gray;
        private Color _appliedAccent = Color.Gray;

        /// <summary>
        /// When true the value is set immediately (no animation). It follows the
        /// Windows "Show animations in Windows" accessibility setting
        /// (<see cref="SystemInformation.UIEffectsEnabled"/>), so a user who
        /// turned animations off for motion sensitivity (or a slow machine)
        /// gets instant values. It is re-read when the system preference
        /// changes.
        /// </summary>
        public static bool ReduceMotion { get; set; }

        static KpiCard()
        {
            ReduceMotion = !SystemInformation.UIEffectsEnabled;

            try
            {
                Microsoft.Win32.SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
            }
            catch
            {
                // SystemEvents needs a message loop and an interactive session.
                // Without them the initial value above still applies.
            }
        }

        private static void OnUserPreferenceChanged(
            object sender, Microsoft.Win32.UserPreferenceChangedEventArgs e)
        {
            ReduceMotion = !SystemInformation.UIEffectsEnabled;
        }

        /// <summary>Fraction digits used for money values (from RegionalContext).</summary>
        public static int CurrencyDigits { get; set; } = 2;

        public string CardKey => _key;

        public string CardTitle
        {
            get => _lblTitle.Text;
            set => _lblTitle.Text = value;
        }

        public IconChar Icon
        {
            get => _iconBox.IconChar;
            set => _iconBox.IconChar = value;
        }

        /// <summary>
        /// The accent color of the card. The value is stored as a base color and
        /// adapted for the active theme, so a card keeps its identity in both
        /// light and dark mode.
        /// </summary>
        public Color AccentColor
        {
            get => _appliedAccent;
            set
            {
                _baseAccent = value;
                ApplyAccent(AppColors.AdaptAccent(value));
            }
        }

        public KpiCard() : this("", "", IconChar.QuestionCircle, Color.Gray, 4)
        {
        }

        public KpiCard(string key, string title, IconChar icon, Color accent, int baseElevation = 4)
        {
            _key = key;
            _baseElevation = baseElevation;
            _currentElevation = baseElevation;

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
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                Cursor = Cursors.Hand,
                BackColor = Color.Transparent
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

            _lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = AppColors.InkMuted,
                Location = new Point(74, 24),
                Size = new Size(120, 18),
                AutoSize = false,
                AutoEllipsis = true,
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };

            _lblValue = new Label
            {
                Text = "0",
                Font = new Font("Segoe UI", 24F, FontStyle.Bold),
                ForeColor = accent,
                Location = new Point(22, 70),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                AutoSize = false,
                Size = new Size(250, 40),
                UseMnemonic = false,
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };

            _lblTrend = new Label
            {
                Text = string.Empty,
                Font = new Font("Segoe UI Semibold", 9.5F),
                ForeColor = AppColors.InkMuted,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(Math.Max(22, Width - 100), 28),
                AutoSize = true,
                Visible = false,
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };

            Controls.AddRange([_accentLeft, _iconBadge, _lblTitle, _lblValue, _lblTrend]);

            Resize += (_, _) => UpdateLayout();

            MouseEnter += (_, _) => SetHover(true);
            MouseLeave += (_, _) => SetHover(false);
            foreach (Control c in Controls)
            {
                c.MouseEnter += (_, _) => SetHover(true);
                c.MouseLeave += (_, _) => SetHover(false);
            }

            SetRoundedRegion();

            UpdateLayout();

            HandleCreated += (_, _) => ApplyTheme();
            if (IsHandleCreated)
                ApplyTheme();

            ThemeManager.ThemeChanged += OnThemeChanged;
        }

        private void OnThemeChanged(object? sender, EventArgs e)
        {
            if (IsDisposed) return;
            if (InvokeRequired) { BeginInvoke(ApplyTheme); return; }
            ApplyTheme();
        }

        private void UpdateLayout()
        {
            if (IsDisposed) return;

            int trendLeft = Math.Max(74, Width - _lblTrend.Width - 22);
            _lblTrend.Location = new Point(trendLeft, 28);

            _lblTitle.Width = Math.Max(40, trendLeft - 84);

            _lblValue.Width = Math.Max(40, Width - 44);
            _lblValue.Location = new Point(22, Math.Max(60, Height - _lblValue.Height - 28));
        }

        public void ApplyTheme()
        {
            if (IsDisposed) return;

            ApplyAccent(AppColors.AdaptAccent(_baseAccent));
            BackColor = AppColors.Surface;
            _lblTitle.ForeColor = AppColors.InkMuted;
            Invalidate();
        }

        private void ApplyAccent(Color color)
        {
            _appliedAccent = color;
            if (_accentLeft != null) _accentLeft.BackColor = color;
            if (_lblValue != null) _lblValue.ForeColor = color;
            if (_iconBox != null) _iconBox.IconColor = Color.White;
            if (_iconBadge != null)
            {
                _iconBadge.BackColor = ThemeManager.IsDark
                    ? Blend(AppColors.Surface, color, 0.30f)
                    : Blend(Color.White, color, 0.15f);
            }
        }

        private static Color Blend(Color background, Color accent, float accentShare)
            => Color.FromArgb(
                (int)(background.R + (accent.R - background.R) * accentShare),
                (int)(background.G + (accent.G - background.G) * accentShare),
                (int)(background.B + (accent.B - background.B) * accentShare));

        /// <summary>
        /// Sets the tile from a raw number. The value is formatted here through
        /// <see cref="KpiValueFormatter"/>; no formatted string is ever parsed.
        /// </summary>
        public void SetValue(
            decimal value,
            KpiValueKind kind,
            KpiTrendKind trendKind = KpiTrendKind.None,
            decimal trendPercent = 0,
            int? currencyDigits = null)
        {
            _valueKind = kind;
            _currencyDigits = currencyDigits ?? CurrencyDigits;

            ApplyTrend(trendKind, trendPercent);

            _targetNum = value;

            if (ReduceMotion || !IsHandleCreated)
            {
                _currentNum = _targetNum;
                _lblValue.Text = FormatValue(_currentNum);
                return;
            }

            StartAnimation();
        }

        private void ApplyTrend(KpiTrendKind trendKind, decimal trendPercent)
        {
            var culture = CultureInfo.CurrentCulture;

            switch (trendKind)
            {
                case KpiTrendKind.Change:
                    var up = trendPercent >= 0;
                    var magnitude = KpiValueFormatter.Format(Math.Abs(trendPercent), KpiValueKind.Percent, culture);
                    _lblTrend.Text = up ? $"\u25B2 +{magnitude}" : $"\u25BC {magnitude}";
                    _lblTrend.ForeColor = up ? AppColors.Success : AppColors.Danger;
                    _lblTrend.Visible = true;
                    break;

                case KpiTrendKind.Margin:
                    var margin = KpiValueFormatter.Format(trendPercent, KpiValueKind.Percent, culture);
                    _lblTrend.Text = $"{AppStrings.Margin} {margin}";
                    _lblTrend.ForeColor = AppColors.InkMuted;
                    _lblTrend.Visible = true;
                    break;

                default:
                    _lblTrend.Text = string.Empty;
                    _lblTrend.Visible = false;
                    break;
            }
        }

        private string FormatValue(decimal num)
            => KpiValueFormatter.Format(
                num,
                _valueKind,
                CultureInfo.CurrentCulture,
                _currencyDigits,
                compactSuffixes: LocalizedCompactSuffixes());

        private static KpiCompactSuffixes LocalizedCompactSuffixes()
            => new(
                AppStrings.KpiSuffixThousand,
                AppStrings.KpiSuffixMillion,
                AppStrings.KpiSuffixBillion);

        private void StartAnimation()
        {
            _animTimer?.Stop();
            _animTimer?.Dispose();

            int totalSteps = 20;
            int interval = 16;
            decimal start = _currentNum;
            decimal range = _targetNum - start;
            int step = 0;

            _animTimer = new System.Windows.Forms.Timer { Interval = interval };
            _animTimer.Tick += (_, _) =>
            {
                step++;
                if (step >= totalSteps)
                {
                    _currentNum = _targetNum;
                    _lblValue.Text = FormatValue(_targetNum);
                    _animTimer?.Stop();
                    return;
                }

                double t = (double)step / totalSteps;
                double eased = 1 - Math.Pow(1 - t, 3);
                _currentNum = start + (decimal)((double)range * eased);
                _lblValue.Text = FormatValue(_currentNum);
            };
            _animTimer.Start();
        }

        private void SetHover(bool hovering)
        {
            _currentElevation = hovering ? _baseElevation + 4 : _baseElevation;
            ShadowEngine.Enable(this, _currentElevation);
        }

        private void SetRoundedRegion()
        {
            Resize += (_, _) => ApplyRegion();
            ApplyRegion();
        }

        private void ApplyRegion()
        {
            if (Width <= 0 || Height <= 0) return;

            var r = Math.Min(12, Math.Min(Width, Height) / 2);
            if (r <= 0)
            {
                var stale = Region;
                Region = null;
                stale?.Dispose();
                return;
            }

            using var path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddArc(0, 0, r, r, 180, 90);
            path.AddArc(Width - r, 0, r, r, 270, 90);
            path.AddArc(Width - r, Height - r, r, r, 0, 90);
            path.AddArc(0, Height - r, r, r, 90, 90);
            path.CloseFigure();
            var oldRegion = Region;
            Region = new Region(path);
            oldRegion?.Dispose();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _animTimer?.Stop();
                _animTimer?.Dispose();
                ThemeManager.ThemeChanged -= OnThemeChanged;
            }
            base.Dispose(disposing);
        }
    }
}