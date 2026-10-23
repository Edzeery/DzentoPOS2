using FontAwesome.Sharp;
using DzentoPOS2.WinForms.Events;
using DzentoPOS2.WinForms.Themes;

namespace DzentoPOS2.WinForms.Controls.Header;

public sealed class SettingsButton : IconButton, IThemeAware
{
    private readonly ContextMenuStrip _menu = new();
    private readonly List<SettingsMenuCustomItem> _customItems = new();
    private string _subscriptionTitle = "Subscription";
    private string _subscriptionDetails = "Active plan";
    private Color _normalBackColor = HeaderColors.SurfaceAlt;
    private Color _hoverBackColor = HeaderColors.AccentSoft;
    private bool _canUpgrade = true;

    public SettingsButton()
    {
        Cursor = Cursors.Hand;
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        BackColor = HeaderColors.SurfaceAlt;
        ForeColor = HeaderColors.Ink;
        IconChar = IconChar.Gear;
        IconColor = HeaderColors.Ink;
        IconFont = IconFont.Auto;
        IconSize = 20;
        Size = new Size(42, 42);
        Text = "";
        UseVisualStyleBackColor = false;
        AccessibleName = "Settings";

        _menu.ShowImageMargin = false;
        _menu.ShowCheckMargin = false;
        _menu.BackColor = HeaderColors.Surface;
        _menu.Padding = new Padding(6);
        _menu.Renderer = new HeaderPopupRenderer();
        _menu.MinimumSize = new Size(270, 0);

        Click += (_, _) => ShowSettingsMenu();
        MouseEnter += (_, _) => BackColor = HoverBackColor;
        MouseLeave += (_, _) => BackColor = NormalBackColor;

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

        _normalBackColor = HeaderColors.SurfaceAlt;
        _hoverBackColor = HeaderColors.AccentSoft;
        BackColor = _normalBackColor;
        ForeColor = HeaderColors.Ink;
        IconColor = HeaderColors.Ink;
        _menu.BackColor = HeaderColors.Surface;
        Invalidate();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            ThemeManager.ThemeChanged -= OnThemeChanged;

        base.Dispose(disposing);
    }

    public event EventHandler<SettingsMenuEventArgs>? MenuActionSelected;

    public HeaderPopupPlacement PopupPlacement { get; set; } = HeaderPopupOptions.DefaultPlacement;

    public Color NormalBackColor
    {
        get => _normalBackColor;
        set
        {
            _normalBackColor = value;
            BackColor = value;
        }
    }

    public Color HoverBackColor
    {
        get => _hoverBackColor;
        set => _hoverBackColor = value;
    }

    public string SettingsText { get; set; } = "Settings";

    public string SettingsDescription { get; set; } = "Open application settings";

    public string SubscriptionText { get; set; } = "Subscription";

    public string SubscriptionDescription { get; set; } = "View plan and billing details";

    public string UpgradeText { get; set; } = "Upgrade";

    public string UpgradeDescription { get; set; } = "Upgrade current plan";

    public string LogoutText { get; set; } = "Logout";

    public string LogoutDescription { get; set; } = "End current session";

    public bool CanUpgrade
    {
        get => _canUpgrade;
        set => _canUpgrade = value;
    }

    public void SetSubscriptionInfo(string title, string details)
    {
        _subscriptionTitle = string.IsNullOrWhiteSpace(title) ? "Subscription" : title;
        _subscriptionDetails = string.IsNullOrWhiteSpace(details) ? "Active plan" : details;
    }

    public void SetLicenseInfo(
        string currentPlan,
        string details,
        string? upgradePlan,
        bool canUpgrade)
    {
        _subscriptionTitle = string.IsNullOrWhiteSpace(currentPlan) ? "Subscription" : currentPlan;
        _subscriptionDetails = string.IsNullOrWhiteSpace(details) ? "Active plan" : details;
        _canUpgrade = canUpgrade;

        if (!string.IsNullOrWhiteSpace(upgradePlan))
            UpgradeDescription = upgradePlan;
    }

    public void AddCustomMenuItem(
        string text,
        string description,
        string actionKey,
        bool isDanger = false)
    {
        if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(actionKey))
            return;

        _customItems.Add(new SettingsMenuCustomItem(text, description, actionKey, isDanger, true));
    }

    public void AddInfoMenuItem(string text, string description)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        _customItems.Add(new SettingsMenuCustomItem(text, description, "", false, false));
    }

    public void ClearCustomMenuItems() => _customItems.Clear();

    protected override void OnRightToLeftChanged(EventArgs e)
    {
        base.OnRightToLeftChanged(e);
        _menu.RightToLeft = RightToLeft;
    }

    private void ShowSettingsMenu()
    {
        BuildMenuItems();

        _menu.Width = Math.Max(270, Width);
        _menu.RightToLeft = RightToLeft;

        foreach (ToolStripItem item in _menu.Items)
        {
            item.Width = _menu.Width - _menu.Padding.Horizontal;
            item.RightToLeft = RightToLeft;
        }

        var screenPoint = HeaderPopupPositioner.GetScreenPoint(
            this,
            _menu,
            PopupPlacement,
            HeaderPopupOptions.Gap);
        _menu.Show(screenPoint);
    }

    private void BuildMenuItems()
    {
        _menu.Items.Clear();

        _menu.Items.Add(new HeaderPopupMenuItem(_subscriptionTitle, _subscriptionDetails)
        {
            Enabled = false,
            RightToLeft = RightToLeft
        });
        _menu.Items.Add(new HeaderPopupSeparator());

        var actionsRow = new HeaderSettingsActionsMenuItem(
            SettingsText,
            CanUpgrade ? UpgradeText : SubscriptionText,
            CanUpgrade,
            RightToLeft)
        {
            Width = _menu.Width - _menu.Padding.Horizontal,
            Font = Font
        };
        actionsRow.ActionSelected += (_, e) => MenuActionSelected?.Invoke(this, e);
        _menu.Items.Add(actionsRow);

        foreach (var customItem in _customItems)
        {
            AddMenuItem(
                customItem.Text,
                customItem.Description,
                SettingsMenuAction.Custom,
                customItem.IsDanger,
                customItem.ActionKey,
                customItem.IsEnabled);
        }

        _menu.Items.Add(new HeaderPopupSeparator());
        AddMenuItem(LogoutText, LogoutDescription, SettingsMenuAction.Logout, isDanger: true);
    }

    private void AddMenuItem(
        string text,
        string description,
        SettingsMenuAction action,
        bool isDanger = false,
        string? customActionKey = null,
        bool enabled = true)
    {
        var item = new HeaderPopupMenuItem(text, description)
        {
            Tag = action,
            IsDanger = isDanger,
            RightToLeft = RightToLeft,
            Enabled = enabled
        };

        if (enabled)
            item.Click += (_, _) => MenuActionSelected?.Invoke(this, new SettingsMenuEventArgs(action, customActionKey));

        _menu.Items.Add(item);
    }

    private sealed record SettingsMenuCustomItem(
        string Text,
        string Description,
        string ActionKey,
        bool IsDanger,
        bool IsEnabled);
}
