using DzentoPOS2.WinForms.Events;
using DzentoPOS2.WinForms.Models;
using DzentoPOS2.WinForms.Themes;
using EZControls.Controls.Inputs;
using FontAwesome.Sharp;

namespace DzentoPOS2.WinForms.Controls.Header;

public sealed class MainHeaderBar : UserControl, IThemeAware
{
    private const int HeaderHeight = 72;
    private const int RightPanelWidth = 665;
    private const int TitlePanelWidth = 250;

    private readonly EZTextBox _searchBox = new();
    private readonly EZComboBox _languageBox = new();
    private readonly TableLayoutPanel _root = new();
    private HeaderPopupPlacement _popupPlacement = HeaderPopupOptions.DefaultPlacement;

    public MainHeaderBar()
    {
        DoubleBuffered = true;
        BackColor = HeaderColors.Surface;
        Dock = DockStyle.Fill;
        MinimumSize = new Size(760, HeaderHeight);
        Padding = new Padding(18, 12, 18, 12);

        _menuButton = new IconButton();
        TitleLabel = new Label();

        BranchSelector = new BranchSelectorControl
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 10, 0)
        };

        Notifications = new NotificationButton { Anchor = AnchorStyles.None };
        Settings = new SettingsButton { Anchor = AnchorStyles.None };
        UserMenu = new UserMenuControl { Dock = DockStyle.Fill };

        BuildLayout();
        WireEvents();

        ThemeManager.ThemeChanged += OnThemeChanged;
    }

    private void OnThemeChanged(object? sender, EventArgs e)
    {
        if (IsDisposed) return;
        if (InvokeRequired) { BeginInvoke(ApplyTheme); return; }
        ApplyTheme();
    }

    public void ApplyTheme()
    {
        if (IsDisposed) return;

        BackColor = HeaderColors.Surface;
        TitleLabel.ForeColor = HeaderColors.Ink;
        _menuButton.IconColor = HeaderColors.Ink;
        _searchBox.BackColor = HeaderColors.Surface;
        _searchBox.BorderColor = HeaderColors.Border;
        _searchBox.BorderFocusColor = HeaderColors.Accent;
        _searchBox.ForeColor = HeaderColors.MutedInk;
        _searchBox.PlaceholderColor = HeaderColors.MutedInk;

        HeaderComboBoxStyle.Apply(_languageBox);

        BranchSelector.ApplyTheme();
        Notifications.ApplyTheme();
        Settings.ApplyTheme();
        UserMenu.ApplyTheme();

        Invalidate();
    }

    public IconButton _menuButton { get; }

    public Label TitleLabel { get; }

    public BranchSelectorControl BranchSelector { get; }

    public NotificationButton Notifications { get; }

    public SettingsButton Settings { get; }

    public UserMenuControl UserMenu { get; }

    public event EventHandler? MenuClicked;

    public event EventHandler? SettingsClicked;

    public event EventHandler<SettingsMenuEventArgs>? SettingsMenuActionSelected;

    public event EventHandler? SearchSubmitted;

    public event EventHandler? LanguageChanged;

    public string SearchText => _searchBox.Text?.Trim() ?? string.Empty;

    public HeaderPopupPlacement PopupPlacement
    {
        get => _popupPlacement;
        set
        {
            _popupPlacement = value;
            BranchSelector.PopupPlacement = value;
            Notifications.PopupPlacement = value;
            Settings.PopupPlacement = value;
            UserMenu.PopupPlacement = value;
            _languageBox.DropDownPlacement = ToEzPlacement(value);
        }
    }

    public int SelectedLanguageIndex
    {
        get => _languageBox.SelectedIndex;
        set
        {
            if (value >= 0 && value < _languageBox.Items.Length)
                _languageBox.SelectedIndex = value;
        }
    }

    public void SetUser(UserInfoViewModel user) => UserMenu.SetUser(user);

    public void SetBranches(IEnumerable<BranchViewModel> branches, Guid? selectedBranchId = null) =>
        BranchSelector.SetBranches(branches, selectedBranchId);

    public void SetNotifications(IEnumerable<NotificationItem> notifications) =>
        Notifications.SetNotifications(notifications);

    public void SetSubscriptionInfo(string title, string details) =>
        Settings.SetSubscriptionInfo(title, details);

    public void AddSettingsMenuItem(
        string text,
        string description,
        string actionKey,
        bool isDanger = false) =>
        Settings.AddCustomMenuItem(text, description, actionKey, isDanger);

    public void ClearSettingsMenuItems() => Settings.ClearCustomMenuItems();

    private void BuildLayout()
    {
        ConfigureMenuButton();
        ConfigureTitleLabel();
        ConfigureSearchBox();
        ConfigureLanguageBox();

        var titlePanel = CreateTitlePanel();
        var rightPanel = CreateRightPanel();

        _root.Dock = DockStyle.Fill;
        _root.ColumnCount = 3;
        _root.RowCount = 1;
        _root.BackColor = Color.Transparent;
        _root.Margin = Padding.Empty;
        _root.Padding = Padding.Empty;
        _root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        _root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, TitlePanelWidth));
        _root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, RightPanelWidth));
        _root.Controls.Add(titlePanel, 0, 0);
        _root.Controls.Add(_searchBox, 1, 0);
        _root.Controls.Add(rightPanel, 2, 0);

        Controls.Add(_root);
    }

    private void ConfigureMenuButton()
    {
        _menuButton.Anchor = AnchorStyles.Left;
        _menuButton.Cursor = Cursors.Hand;
        _menuButton.FlatAppearance.BorderSize = 0;
        _menuButton.FlatStyle = FlatStyle.Flat;
        _menuButton.IconChar = IconChar.Bars;
        _menuButton.IconColor = HeaderColors.Ink;
        _menuButton.IconFont = IconFont.Auto;
        _menuButton.IconSize = 28;
        _menuButton.Margin = new Padding(0);
        _menuButton.Name = "_menuButton";
        _menuButton.Size = new Size(34, 34);
        _menuButton.TabIndex = 0;
        _menuButton.Text = "";
        _menuButton.UseVisualStyleBackColor = true;
    }

    private void ConfigureTitleLabel()
    {
        TitleLabel.AutoEllipsis = true;
        TitleLabel.AutoSize = false;
        TitleLabel.Dock = DockStyle.Fill;
        TitleLabel.Font = new Font("Segoe UI Semibold", 13F);
        TitleLabel.ForeColor = HeaderColors.Ink;
        TitleLabel.Margin = new Padding(8, 0, 0, 0);
        TitleLabel.Name = "TitleLabel";
        TitleLabel.TabIndex = 1;
        TitleLabel.Text = "Dashboard";
        TitleLabel.TextAlign = ContentAlignment.MiddleLeft;
        TitleLabel.UseMnemonic = false;
        TitleLabel.TextChanged += (_, _) =>
        {
            TitleLabel.Text = TitleLabel.Text
                .Replace(Environment.NewLine, " ")
                .Replace('\n', ' ')
                .Replace('\r', ' ');
        };
    }

    private void ConfigureSearchBox()
    {
        _searchBox.BackColor = HeaderColors.Surface;
        _searchBox.BorderColor = HeaderColors.Border;
        _searchBox.BorderFocusColor = HeaderColors.Accent;
        _searchBox.BorderRadius = 12;
        _searchBox.BorderSize = 1;
        _searchBox.Dock = DockStyle.Fill;
        _searchBox.Font = new Font("Segoe UI", 10F);
        _searchBox.ForeColor = HeaderColors.MutedInk;
        _searchBox.Margin = new Padding(12, 6, 12, 6);
        _searchBox.Padding = new Padding(12, 6, 12, 6);
        _searchBox.PlaceholderColor = Color.FromArgb(150, 160, 166);
        _searchBox.PlaceholderText = "Search";
    }

    private void ConfigureLanguageBox()
    {
        HeaderComboBoxStyle.Apply(_languageBox);
        _languageBox.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        _languageBox.Items = new[] { "العربية", "English", "Français" };
        _languageBox.Margin = new Padding(0, 5, 10, 5);
    }

    private TableLayoutPanel CreateTitlePanel()
    {
        var titlePanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = Color.Transparent,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };

        titlePanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        titlePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 42));
        titlePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        titlePanel.Controls.Add(_menuButton, 0, 0);
        titlePanel.Controls.Add(TitleLabel, 1, 0);

        return titlePanel;
    }

    private TableLayoutPanel CreateRightPanel()
    {
        var rightPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 5,
            RowCount = 1,
            BackColor = Color.Transparent,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };

        rightPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        rightPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 230));
        rightPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 118));
        rightPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50));
        rightPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50));
        rightPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        rightPanel.Controls.Add(BranchSelector, 0, 0);
        rightPanel.Controls.Add(_languageBox, 1, 0);
        rightPanel.Controls.Add(Notifications, 2, 0);
        rightPanel.Controls.Add(Settings, 3, 0);
        rightPanel.Controls.Add(UserMenu, 4, 0);

        return rightPanel;
    }

    private void WireEvents()
    {
        _menuButton.Click += (_, _) => MenuClicked?.Invoke(this, EventArgs.Empty);
        Settings.MenuActionSelected += (_, e) =>
        {
            SettingsMenuActionSelected?.Invoke(this, e);
            if (e.Action == SettingsMenuAction.OpenSettings)
                SettingsClicked?.Invoke(this, EventArgs.Empty);
        };
        _languageBox.SelectedIndexChanged += (_, _) => LanguageChanged?.Invoke(this, EventArgs.Empty);
        _searchBox.KeyDown += (_, e) =>
        {
            if (e.KeyCode != Keys.Enter)
                return;

            e.SuppressKeyPress = true;
            SearchSubmitted?.Invoke(this, EventArgs.Empty);
        };
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);

        _searchBox.Visible = Width >= 980;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        using var pen = new Pen(HeaderColors.Border);
        e.Graphics.DrawLine(pen, 0, Height - 1, Width, Height - 1);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            ThemeManager.ThemeChanged -= OnThemeChanged;

        base.Dispose(disposing);
    }

    private static EZDropDownPlacement ToEzPlacement(HeaderPopupPlacement placement) =>
        placement switch
        {
            HeaderPopupPlacement.Top => EZDropDownPlacement.Top,
            HeaderPopupPlacement.Left => EZDropDownPlacement.Left,
            HeaderPopupPlacement.Right => EZDropDownPlacement.Right,
            _ => EZDropDownPlacement.Bottom
        };
}
