using FontAwesome.Sharp;
using DzentoPOS2.WinForms.ServicesUI;
using DzentoPOS2.WinForms.Themes;

namespace DzentoPOS2.WinForms.Controls
{
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
        private string _prefix = "";
        private string _suffix = "";
        private decimal _currentNum;
        private decimal _targetNum;
        private System.Windows.Forms.Timer? _animTimer;
        private Color _baseAccent = Color.Gray;
        private Color _appliedAccent = Color.Gray;

        public string CardKey => _key;
        public string CardTitle { get => _lblTitle.Text; set => _lblTitle.Text = value; }
        public string CardValue { get => _lblValue.Text; set => _lblValue.Text = value; }
        public string CardTrend
        {
            get
            {
                var parts = _lblTrend.Text.Split(' ');
                return parts.Length > 1 ? string.Join(" ", parts, 1, parts.Length - 1) : _lblTrend.Text;
            }
            set => UpdateTrendText(value, _isUp);
        }

        public IconChar Icon { get => _iconBox.IconChar; set => _iconBox.IconChar = value; }

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

        private bool _isUp = true;

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
                AutoSize = true,
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };

            _lblTrend = new Label
            {
                Text = "\u25B2 0%",
                Font = new Font("Segoe UI Semibold", 9.5F),
                ForeColor = AppColors.InkMuted,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(Math.Max(22, Width - 100), 28),
                AutoSize = true,
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

            // Keep the title clear of the trend badge on narrow cards.
            _lblTitle.Width = Math.Max(40, trendLeft - 84);

            _lblValue.Location = new Point(22, Math.Max(60, Height - _lblValue.Height - 28));
        }

        public void ApplyTheme()
        {
            if (IsDisposed) return;

            ApplyAccent(AppColors.AdaptAccent(_baseAccent));
            BackColor = AppColors.Surface;
            _lblTitle.ForeColor = AppColors.InkMuted;
            _lblTrend.ForeColor = _lblTrend.Text.Contains('%') ? (_lblTrend.Text.Contains("\u25B2") ? AppColors.Success : AppColors.Danger) : AppColors.InkMuted;
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
                // A light pastel badge in light mode, a tinted dark badge in dark
                // mode: a white pastel patch would glow on the dark card.
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

        public void SetData(string value, string trend, bool isUp)
        {
            _isUp = isUp;
            UpdateTrendText(trend, isUp);
            AnimateValue(value);
        }

        private void UpdateTrendText(string trend, bool isUp)
        {
            bool isPercent = trend.Contains('%');
            _lblTrend.Text = isPercent ? $"{(isUp ? "\u25B2" : "\u25BC")} {trend}" : trend;
            _lblTrend.ForeColor = isPercent
                ? (isUp ? AppColors.Success : AppColors.Danger)
                : AppColors.InkMuted;
        }

        private void AnimateValue(string newValue)
        {
            var parsed = TryParseNumeric(newValue);
            if (parsed.HasValue)
            {
                _targetNum = parsed.Value;
                StartAnimation();
            }
            else
            {
                _lblValue.Text = newValue;
            }
        }

        private decimal? TryParseNumeric(string text)
        {
            if (string.IsNullOrEmpty(text)) return null;
            var cleaned = new string(text.Where(c => char.IsDigit(c) || c == '.' || c == '-' || c == ',').ToArray());
            cleaned = cleaned.Replace(",", "");
            if (decimal.TryParse(cleaned, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var result))
            {
                _prefix = text.TrimEnd(cleaned.ToCharArray()).Trim();
                _suffix = text.Substring(text.Length - (text.Length - text.LastIndexOfAny("0123456789".ToCharArray()) - 1)).Trim();
                if (_suffix == text) _suffix = "";
                return result;
            }
            return null;
        }

        private void StartAnimation()
        {
            _animTimer?.Stop();
            _animTimer?.Dispose();

            int totalSteps = 24;
            int interval = 16;
            decimal start = _currentNum;
            decimal range = _targetNum - start;
            int step = 0;

            _animTimer = new System.Windows.Forms.Timer
            {
                Interval = interval
            };
            _animTimer.Tick += (_, _) =>
            {
                step++;
                if (step >= totalSteps)
                {
                    _currentNum = _targetNum;
                    _lblValue.Text = FormatDisplayValue(_targetNum);
                    _animTimer?.Stop();
                    return;
                }
                double t = (double)step / totalSteps;
                double eased = 1 - Math.Pow(1 - t, 3);
                _currentNum = start + (decimal)((double)range * eased);
                _lblValue.Text = FormatDisplayValue(_currentNum);
            };
            _animTimer.Start();
        }

        private string FormatDisplayValue(decimal num)
        {
            var formatted = num.ToString("N0");
            return $"{_prefix}{formatted}{_suffix}".Trim();
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
                // Still being laid out: GDI+ rejects a zero sized arc.
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
