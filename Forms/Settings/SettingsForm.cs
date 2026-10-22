using DzentoPOS2.Localization.Resources;
using DzentoPOS2.Localization.Services.Interfaces;
using DzentoPOS2.Shared.Helpers;
using DzentoPOS2.WinForms.Configurations;
using DzentoPOS2.WinForms.Controls;
using DzentoPOS2.WinForms.Helpers;
using DzentoPOS2.WinForms.Services;
using DzentoPOS2.WinForms.Services.Barcodes;
using DzentoPOS2.WinForms.Themes;

namespace DzentoPOS2.WinForms.Forms.Settings;

public partial class SettingsForm : LocalizableFormBase
{
    private readonly ICurrentSettingsService _currentSettings;

    private readonly TableLayoutPanel _root;
    private readonly TableLayoutPanel _page;
    private readonly List<SettingsCard> _cards = new();
    private int _layoutColumns;

    private readonly SettingsCard _brandingCard;
    private readonly SettingsCard _regionalCard;
    private readonly SettingsCard _appearanceCard;
    private readonly SettingsCard _printingCard;
    private readonly SettingsCard _barcodeCard;
    private readonly SettingsCard _aboutCard;

    private readonly PictureBox _picLogo;
    private readonly Label _lblLogoCaption;
    private readonly ThemedButton _btnChooseLogo;
    private readonly ThemedButton _btnRemoveLogo;
    private readonly TextBox _txtStoreName;

    private readonly ThemedComboBox _cmbLanguage;
    private readonly ThemedComboBox _cmbCountry;
    private readonly ThemedComboBox _cmbTimeZone;
    private readonly ThemedComboBox _cmbCurrency;

    private readonly CheckBox _chkDarkMode;
    private readonly CheckBox _chkRememberMe;

    private readonly ThemedComboBox _cmbPrinter;
    private readonly ThemedComboBox _cmbPaperSize;
    private readonly ThemedComboBox _cmbOrientation;
    private readonly ThemedComboBox _cmbReceiptWidth;
    private readonly ThemedButton _btnTestPrint;

    private readonly CheckBox _chkCameraBarcode;
    private readonly ThemedComboBox _cmbCamera;
    private readonly ThemedButton _btnRefreshCameras;

    private readonly Label _lblVersion;

    private readonly ThemedButton _btnSave;
    private readonly ThemedButton _btnReset;

    private AppSettings _settings = new();
    private string _languageCode = "ar";

    private readonly List<LanguageItem> _languages = new();
    private readonly List<CountryItem> _countries = new();
    private readonly List<TimeZoneItem> _timeZones = new();
    private readonly List<CurrencyItem> _currencies = new();
    private readonly List<PrinterItem> _printers = new();
    private readonly List<PaperItem> _papers = new();
    private readonly List<ReceiptItem> _receipts = new();
    private readonly List<CameraItem> _cameras = new();

    public SettingsForm(
        ICurrentSettingsService currentSettings,
        ILanguageService languageService)
        : base(languageService)
    {
        _currentSettings = currentSettings;

        BackColor = AppColors.Background;
        Size = new Size(940, 720);
        // The page is responsive, so the window can also be hosted inside a
        // narrow shell (375px) without forcing the shell to grow.
        MinimumSize = new Size(320, 420);

        _root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = AppColors.Background,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        _root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        _root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        _page = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 0,
            GrowStyle = TableLayoutPanelGrowStyle.AddRows,
            AutoScroll = true,
            BackColor = AppColors.Background,
            Margin = Padding.Empty,
            Padding = new Padding(24, 20, 24, 20),
            Tag = "background"
        };

        // ── Branding ───────────────────────────────────────────────────────
        _brandingCard = new SettingsCard(AppStrings.StoreBranding, AppStrings.SettingsDescription);

        _picLogo = new PictureBox
        {
            Size = new Size(96, 96),
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = AppColors.SurfaceAlt,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 16, 0)
        };

        _lblLogoCaption = new Label
        {
            Text = AppStrings.StoreLogo,
            Font = AppStyle.BodyFont,
            ForeColor = AppColors.Ink,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 8)
        };

        _btnChooseLogo = ButtonFactory.Primary(AppStrings.ChooseImage, ButtonFactory.ButtonSize.Small);
        _btnChooseLogo.Click += (_, _) => ChooseLogo();
        _btnChooseLogo.Margin = new Padding(0, 0, 8, 0);

        _btnRemoveLogo = ButtonFactory.Neutral(AppStrings.RemoveLogo, ButtonFactory.ButtonSize.Small);
        _btnRemoveLogo.Click += (_, _) => RemoveLogo();
        _btnRemoveLogo.Margin = Padding.Empty;

        var logoButtons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            AutoSize = false,
            BackColor = Color.Transparent,
            Margin = Padding.Empty
        };
        logoButtons.Controls.Add(_btnChooseLogo);
        logoButtons.Controls.Add(_btnRemoveLogo);

        var logoInfo = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = Color.Transparent,
            AutoSize = false,
            Margin = Padding.Empty
        };
        logoInfo.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        logoInfo.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        logoInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        logoInfo.Controls.Add(_lblLogoCaption, 0, 0);
        logoInfo.Controls.Add(logoButtons, 0, 1);

        var logoRow = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = Color.Transparent,
            AutoSize = false,
            Height = 96,
            Margin = new Padding(0, 0, 0, 8)
        };
        logoRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 112));
        logoRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        logoRow.RowStyles.Add(new RowStyle(SizeType.Absolute, 96));
        logoRow.Controls.Add(_picLogo, 0, 0);
        logoRow.Controls.Add(logoInfo, 1, 0);
        _brandingCard.AddControl(logoRow, 96);

        _txtStoreName = new TextBox
        {
            Font = AppStyle.BodyFont,
            BorderStyle = BorderStyle.FixedSingle,
            MaxLength = 120
        };
        _brandingCard.AddFields(new SettingsField("storeName", AppStrings.StoreName, _txtStoreName));

        // ── Regional ───────────────────────────────────────────────────────
        _regionalCard = new SettingsCard(AppStrings.RegionalSettings, AppStrings.SettingsDescription);

        _cmbLanguage = new ThemedComboBox();
        _cmbCountry = new ThemedComboBox();
        _cmbTimeZone = new ThemedComboBox();
        _cmbCurrency = new ThemedComboBox();

        _regionalCard.AddFields(
            new SettingsField("language", AppStrings.Language, _cmbLanguage),
            new SettingsField("country", AppStrings.Country, _cmbCountry));
        _regionalCard.AddFields(
            new SettingsField("timeZone", AppStrings.TimeZone, _cmbTimeZone),
            new SettingsField("currency", AppStrings.Currency, _cmbCurrency));

        // ── Appearance ─────────────────────────────────────────────────────
        _appearanceCard = new SettingsCard(AppStrings.Appearance, AppStrings.SettingsDescription);

        _chkDarkMode = new CheckBox
        {
            Text = AppStrings.DarkMode,
            Font = AppStyle.BodyFont,
            AutoSize = true,
            Margin = new Padding(0, 0, 24, 0)
        };

        _chkRememberMe = new CheckBox
        {
            Text = AppStrings.RememberMe,
            Font = AppStyle.BodyFont,
            AutoSize = true,
            Margin = Padding.Empty
        };

        _appearanceCard.AddFields(
            new SettingsField("darkMode", AppStrings.DarkMode, _chkDarkMode),
            new SettingsField("rememberMe", AppStrings.RememberMe, _chkRememberMe));

        // ── Printing ───────────────────────────────────────────────────────
        _printingCard = new SettingsCard(AppStrings.Printing, AppStrings.SettingsDescription);

        _cmbPrinter = new ThemedComboBox();
        _cmbPaperSize = new ThemedComboBox();
        _cmbOrientation = new ThemedComboBox();
        _cmbReceiptWidth = new ThemedComboBox();

        _cmbPrinter.SelectedIndexChanged += (_, _) => RefreshPaperSizes();

        _printingCard.AddFields(
            new SettingsField("printer", AppStrings.PrinterName, _cmbPrinter),
            new SettingsField("paperSize", AppStrings.PaperSize, _cmbPaperSize));
        _printingCard.AddFields(
            new SettingsField("orientation", AppStrings.Orientation, _cmbOrientation),
            new SettingsField("receiptWidth", AppStrings.ReceiptWidth, _cmbReceiptWidth));

        _btnTestPrint = ButtonFactory.Neutral(AppStrings.TestPrint, ButtonFactory.ButtonSize.Small);
        _btnTestPrint.Click += (_, _) => PrintTestPage();
        _printingCard.AddControl(_btnTestPrint);

        // ── Barcode ────────────────────────────────────────────────────────
        _barcodeCard = new SettingsCard(AppStrings.BarcodeScanning, AppStrings.ScanHint);

        _chkCameraBarcode = new CheckBox
        {
            Text = AppStrings.EnableCameraScanning,
            Font = AppStyle.BodyFont,
            AutoSize = true,
            Margin = new Padding(0, 0, 24, 0)
        };

        _cmbCamera = new ThemedComboBox();
        _btnRefreshCameras = ButtonFactory.Neutral(AppStrings.Refresh, ButtonFactory.ButtonSize.Small);
        _btnRefreshCameras.Click += (_, _) => LoadCameras();

        _barcodeCard.AddFields(
            new SettingsField("cameraEnabled", AppStrings.EnableCameraScanning, _chkCameraBarcode),
            new SettingsField("cameraDevice", AppStrings.CameraDevice, _cmbCamera));
        _barcodeCard.AddControl(_btnRefreshCameras);

        // ── About ──────────────────────────────────────────────────────────
        _aboutCard = new SettingsCard(AppStrings.About, AppStrings.SettingsDescription);

        _lblVersion = new Label
        {
            Text = $"{AppStrings.Version}: {Application.ProductVersion}   |   DzentoPOS2",
            Font = AppStyle.BodyFont,
            ForeColor = AppColors.InkMuted,
            AutoSize = true,
            Margin = Padding.Empty
        };
        _aboutCard.AddControl(_lblVersion);

        _cards.Add(_brandingCard);
        _cards.Add(_regionalCard);
        _cards.Add(_appearanceCard);
        _cards.Add(_printingCard);
        _cards.Add(_barcodeCard);
        _cards.Add(_aboutCard);

        // ── Footer ─────────────────────────────────────────────────────────
        var footer = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = AppColors.Surface,
            Padding = new Padding(24, 12, 24, 12),
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Tag = "surface"
        };

        _btnSave = ButtonFactory.Primary(AppStrings.Save, ButtonFactory.ButtonSize.Medium);
        _btnSave.Click += (_, _) => SaveSettings();

        _btnReset = ButtonFactory.Neutral(AppStrings.ResetDefaults, ButtonFactory.ButtonSize.Medium);
        _btnReset.Click += (_, _) => ResetSettings();

        var footerButtons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            BackColor = Color.Transparent,
            Margin = Padding.Empty
        };
        footerButtons.Controls.Add(_btnSave);
        footerButtons.Controls.Add(_btnReset);
        footer.Controls.Add(footerButtons);

        _root.Controls.Add(_page, 0, 0);
        _root.Controls.Add(footer, 0, 1);
        Controls.Add(_root);

        _page.Resize += (_, _) => LayoutCards();
        LayoutCards();
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        _settings = _currentSettings.Current;
        _languageCode = _settings.Language;

        LoadLanguages();
        LoadCountries();
        LoadTimeZones();
        LoadCurrencies();
        LoadPrinters();
        LoadOrientations();
        LoadReceiptWidths();
        LoadCameras();

        SelectValue(_cmbLanguage, _languages, l => l.Code, _settings.Language);
        SelectValue(_cmbCountry, _countries, c => c.Code, _settings.CountryCode);
        SelectValue(_cmbTimeZone, _timeZones, z => z.Id, _settings.TimeZoneId);
        SelectValue(_cmbCurrency, _currencies, c => c.Code, _settings.CurrencyCode);
        SelectValue(_cmbPrinter, _printers, p => p.Name, _settings.PrinterName);
        SelectValue(_cmbPaperSize, _papers, p => p.Name, _settings.PaperSize);

        _cmbOrientation.SelectedIndex = _settings.Landscape ? 1 : 0;

        SelectValue(_cmbReceiptWidth, _receipts, r => r.Value, _settings.ReceiptWidthMm.ToString());
        SelectValue(_cmbCamera, _cameras, c => c.Moniker, _settings.BarcodeCameraDevice);

        _txtStoreName.Text = _settings.StoreName;
        _chkDarkMode.Checked = ThemeManager.Mode == ThemeMode.Dark;
        _chkRememberMe.Checked = _settings.RememberMe;
        _chkCameraBarcode.Checked = _settings.CameraBarcodeEnabled;

        RefreshLogo();
        LayoutCards();
    }

    protected override void LoadText()
    {
        Text = AppStrings.Settings;

        _brandingCard.Title = AppStrings.StoreBranding;
        _brandingCard.SetFieldLabel("storeName", AppStrings.StoreName);
        _lblLogoCaption.Text = AppStrings.StoreLogo;
        _btnChooseLogo.Text = AppStrings.ChooseImage;
        _btnRemoveLogo.Text = AppStrings.RemoveLogo;

        _regionalCard.Title = AppStrings.RegionalSettings;
        _regionalCard.SetFieldLabel("language", AppStrings.Language);
        _regionalCard.SetFieldLabel("country", AppStrings.Country);
        _regionalCard.SetFieldLabel("timeZone", AppStrings.TimeZone);
        _regionalCard.SetFieldLabel("currency", AppStrings.Currency);

        _appearanceCard.Title = AppStrings.Appearance;
        _appearanceCard.SetFieldLabel("darkMode", AppStrings.DarkMode);
        _appearanceCard.SetFieldLabel("rememberMe", AppStrings.RememberMe);
        _chkDarkMode.Text = AppStrings.DarkMode;
        _chkRememberMe.Text = AppStrings.RememberMe;

        _printingCard.Title = AppStrings.Printing;
        _printingCard.SetFieldLabel("printer", AppStrings.PrinterName);
        _printingCard.SetFieldLabel("paperSize", AppStrings.PaperSize);
        _printingCard.SetFieldLabel("orientation", AppStrings.Orientation);
        _printingCard.SetFieldLabel("receiptWidth", AppStrings.ReceiptWidth);
        _btnTestPrint.Text = AppStrings.TestPrint;

        _barcodeCard.Title = AppStrings.BarcodeScanning;
        _barcodeCard.Hint = AppStrings.ScanHint;
        _barcodeCard.SetFieldLabel("cameraEnabled", AppStrings.EnableCameraScanning);
        _barcodeCard.SetFieldLabel("cameraDevice", AppStrings.CameraDevice);
        _chkCameraBarcode.Text = AppStrings.EnableCameraScanning;
        _btnRefreshCameras.Text = AppStrings.Refresh;

        _aboutCard.Title = AppStrings.About;
        _lblVersion.Text = $"{AppStrings.Version}: {Application.ProductVersion}   |   DzentoPOS2";

        _btnSave.Text = AppStrings.Save;
        _btnReset.Text = AppStrings.ResetDefaults;

        // The country and language captions follow the active language.
        LoadLanguages();
        LoadCountries();
        LoadOrientations();

        SelectValue(_cmbLanguage, _languages, l => l.Code, _settings.Language);
        SelectValue(_cmbCountry, _countries, c => c.Code, _settings.CountryCode);
    }

    protected override void OnLanguageChangedAfterLoadText()
    {
        SelectValue(_cmbLanguage, _languages, l => l.Code, _settings.Language);
        SelectValue(_cmbCountry, _countries, c => c.Code, _settings.CountryCode);
    }

    // ── Data loading ───────────────────────────────────────────────────────

    private void LoadLanguages()
    {
        _languages.Clear();
        _languages.AddRange(new[]
        {
            new LanguageItem("ar", AppStrings.LanguageArabic),
            new LanguageItem("en", AppStrings.LanguageEnglish),
            new LanguageItem("fr", AppStrings.LanguageFrench)
        });

        _cmbLanguage.Bind(_languages, "Display", "Code");
    }

    private void LoadCountries()
    {
        _countries.Clear();
        _countries.AddRange(RegionalCatalog.Countries
            .Select(c => new CountryItem(
                c.Code,
                RegionalCatalog.GetCountryName(c.Code, _languageCode))));

        _cmbCountry.Bind(_countries, "Display", "Code");
    }

    private void LoadTimeZones()
    {
        _timeZones.Clear();
        _timeZones.AddRange(RegionalCatalog.GetTimeZones()
            .Select(z => new TimeZoneItem(z.Id, FormatTimeZone(z))));

        _cmbTimeZone.Bind(_timeZones, "Display", "Id");
    }

    private void LoadCurrencies()
    {
        _currencies.Clear();
        _currencies.AddRange(RegionalCatalog.Currencies
            .Select(c => new CurrencyItem(
                c.Code,
                $"{c.Code} - {c.Symbol}",
                RegionalCatalog.GetCurrencyName(c.Code, _languageCode))));

        _cmbCurrency.Bind(_currencies, "Display", "Code");
    }

    private void LoadPrinters()
    {
        _printers.Clear();
        _printers.Add(new PrinterItem(string.Empty, AppStrings.SystemDefault));
        _printers.AddRange(PrinterService.GetInstalledPrinters()
            .Select(p => new PrinterItem(p, p)));

        _cmbPrinter.Bind(_printers, "Display", "Name");
    }

    private void LoadOrientations()
    {
        _cmbOrientation.Bind(new List<OrientationItem>
        {
            new("P", AppStrings.Portrait),
            new("L", AppStrings.Landscape)
        }, "Display", "Code");
    }

    private void LoadReceiptWidths()
    {
        _receipts.Clear();
        _receipts.AddRange(new[]
        {
            new ReceiptItem("58", "58 mm"),
            new ReceiptItem("80", "80 mm"),
            new ReceiptItem("210", "A4 (210 mm)")
        });

        _cmbReceiptWidth.Bind(_receipts, "Display", "Value");
    }

    private void LoadCameras()
    {
        _cameras.Clear();
        _cameras.Add(new CameraItem(string.Empty, AppStrings.NoCameraFound));

        try
        {
            _cameras.AddRange(BarcodeCameraService.GetDevices()
                .Select(c => new CameraItem(c.Moniker, c.Name)));
        }
        catch
        {
            // No camera subsystem - the list keeps the "no camera" entry.
        }

        _cmbCamera.Bind(_cameras, "Display", "Moniker");
    }

    private void RefreshPaperSizes()
    {
        string printer = _cmbPrinter.SelectedItem is PrinterItem item ? item.Name : string.Empty;

        _papers.Clear();
        _papers.Add(new PaperItem(string.Empty, AppStrings.SystemDefault));
        _papers.AddRange(PrinterService.GetPaperSizes(printer)
            .Select(p => new PaperItem(p, p)));

        _cmbPaperSize.Bind(_papers, "Display", "Name");

        if (!string.IsNullOrWhiteSpace(_settings.PaperSize))
            SelectValue(_cmbPaperSize, _papers, p => p.Name, _settings.PaperSize);
    }

    // ── Actions ────────────────────────────────────────────────────────────

    private void ChooseLogo()
    {
        using var dialog = new OpenFileDialog
        {
            Title = AppStrings.ChooseImage,
            Filter = "Images|*.png;*.jpg;*.jpeg;*.bmp;*.gif|All files|*.*"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        try
        {
            StoreBranding.SaveLogo(dialog.FileName);
            _settings.StoreLogoPath = StoreBranding.LogoFile;
            RefreshLogo();
        }
        catch (Exception ex)
        {
            AppMessageBox.Error(ex.Message);
        }
    }

    private void RemoveLogo()
    {
        StoreBranding.ClearLogo();
        _settings.StoreLogoPath = string.Empty;
        RefreshLogo();
    }

    private void RefreshLogo()
    {
        _picLogo.Image?.Dispose();
        _picLogo.Image = StoreBranding.LoadLogo();
    }

    private void PrintTestPage()
    {
        var request = new PrintRequest(
            _cmbPrinter.SelectedItem is PrinterItem printer ? printer.Name : string.Empty,
            _cmbPaperSize.SelectedItem is PaperItem paper ? paper.Name : string.Empty,
            _cmbOrientation.SelectedItem is OrientationItem orientation && orientation.Code == "L",
            int.TryParse(_cmbReceiptWidth.SelectedItem is ReceiptItem receipt ? receipt.Value : "80", out int width) ? width : 80,
            string.IsNullOrWhiteSpace(_txtStoreName.Text) ? "DzentoPOS2" : _txtStoreName.Text,
            _cmbCurrency.SelectedItem is CurrencyItem currency ? currency.Code : "SAR",
            AppStrings.TestPrint,
            AppStrings.Printing);

        if (PrinterService.PrintTestPage(request, out string error))
        {
            AppMessageBox.Success(AppStrings.TestPrintSent);
            return;
        }

        AppMessageBox.Error(string.IsNullOrWhiteSpace(error) ? AppStrings.TestPrintFailed : error);
    }

    private void SaveSettings()
    {
        var settings = CurrentSettingsService.Clone(_settings);

        if (_cmbLanguage.SelectedItem is LanguageItem language)
        {
            settings.Language = language.Code;

            if (!string.Equals(language.Code, _languageCode, StringComparison.OrdinalIgnoreCase))
            {
                _languageCode = language.Code;
                _languageService.SetLanguage(language.Code);
            }
        }

        if (_cmbCountry.SelectedItem is CountryItem country)
            settings.CountryCode = country.Code;

        if (_cmbTimeZone.SelectedItem is TimeZoneItem timeZone)
            settings.TimeZoneId = timeZone.Id;

        if (_cmbCurrency.SelectedItem is CurrencyItem currency)
            settings.CurrencyCode = currency.Code;

        settings.StoreName = _txtStoreName.Text.Trim();
        settings.RememberMe = _chkRememberMe.Checked;
        settings.DarkMode = _chkDarkMode.Checked;
        settings.PrinterName = _cmbPrinter.SelectedItem is PrinterItem printer ? printer.Name : _settings.PrinterName;
        settings.PaperSize = _cmbPaperSize.SelectedItem is PaperItem paper ? paper.Name : _settings.PaperSize;
        settings.Landscape = _cmbOrientation.SelectedItem is OrientationItem orientation && orientation.Code == "L";
        settings.ReceiptWidthMm = int.TryParse(_cmbReceiptWidth.SelectedItem is ReceiptItem receipt ? receipt.Value : _settings.ReceiptWidthMm.ToString(), out int width) ? width : _settings.ReceiptWidthMm;
        settings.CameraBarcodeEnabled = _chkCameraBarcode.Checked;
        settings.BarcodeCameraDevice = _cmbCamera.SelectedItem is CameraItem camera ? camera.Moniker : _settings.BarcodeCameraDevice;

        var desiredMode = _chkDarkMode.Checked ? ThemeMode.Dark : ThemeMode.Light;
        if (ThemeManager.Mode != desiredMode)
            ThemeManager.SetMode(desiredMode);

        // Turning "remember me" off must also drop the stored password.
        if (!settings.RememberMe)
            CredentialStore.Delete();

        _currentSettings.Save(settings);
        _settings = settings;

        AppMessageBox.Success(AppStrings.DataSaved);
    }

    private void ResetSettings()
    {
        if (!AppMessageBox.Confirm(AppStrings.ResetDefaults)) return;

        var defaults = new AppSettings
        {
            Language = _settings.Language,
            RememberMe = false,
            DarkMode = ThemeManager.Mode == ThemeMode.Dark,
            StoreName = "DzentoPOS2",
            CurrencyCode = "DZD",
            CountryCode = "DZ",
            TimeZoneId = "Africa/Algiers",
            ReceiptWidthMm = 80
        };

        CredentialStore.Delete();

        _currentSettings.Save(defaults);
        _settings = defaults;

        _txtStoreName.Text = defaults.StoreName;
        _chkRememberMe.Checked = defaults.RememberMe;
        _chkCameraBarcode.Checked = defaults.CameraBarcodeEnabled;

        SelectValue(_cmbCountry, _countries, c => c.Code, defaults.CountryCode);
        SelectValue(_cmbTimeZone, _timeZones, z => z.Id, defaults.TimeZoneId);
        SelectValue(_cmbCurrency, _currencies, c => c.Code, defaults.CurrencyCode);
        SelectValue(_cmbPrinter, _printers, p => p.Name, defaults.PrinterName);
        SelectValue(_cmbPaperSize, _papers, p => p.Name, defaults.PaperSize);
        SelectValue(_cmbCamera, _cameras, c => c.Moniker, defaults.BarcodeCameraDevice);

        AppMessageBox.Success(AppStrings.DataSaved);
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private void LayoutCards()
    {
        int available = _page.ClientSize.Width - _page.Padding.Horizontal;
        if (available <= 0) return;

        // Responsive grid: two columns on a wide window, one column on a
        // narrow one. The grid owns the width of the cards, the cards only
        // report the height their content needs.
        int columns = available >= 980 ? 2 : 1;

        if (columns != _layoutColumns)
        {
            _layoutColumns = columns;
            RebuildGrid(columns);
        }

        int columnWidth = (available - (columns - 1) * 16) / columns;
        int innerWidth = Math.Max(120, columnWidth - 40 - 16);

        // Every card of a row shares the height of the tallest one.
        var heights = new int[(_cards.Count + columns - 1) / columns];

        for (int i = 0; i < _cards.Count; i++)
        {
            SettingsCard card = _cards[i];
            card.Dock = DockStyle.Fill;
            card.Margin = new Padding(0, 0, 16, 16);

            int height = card.ApplyWidth(innerWidth);
            int row = i / columns;
            heights[row] = Math.Max(heights[row], height);
        }

        for (int row = 0; row < heights.Length; row++)
        {
            if (_page.RowStyles.Count <= row) continue;
            _page.RowStyles[row] = new RowStyle(SizeType.Absolute, heights[row] + 16);
        }
    }

    private void RebuildGrid(int columns)
    {
        _page.SuspendLayout();
        _page.Controls.Clear();
        _page.RowStyles.Clear();
        _page.ColumnStyles.Clear();
        _page.ColumnCount = columns;

        for (int i = 0; i < columns; i++)
            _page.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / columns));

        for (int i = 0; i < _cards.Count; i++)
        {
            int row = i / columns;
            int column = i % columns;

            while (_page.RowStyles.Count <= row)
                _page.RowStyles.Add(new RowStyle(SizeType.Absolute, 200));

            SettingsCard card = _cards[i];
            card.Dock = DockStyle.Fill;
            card.Margin = new Padding(0, 0, 16, 16);
            _page.Controls.Add(card, column, row);
        }

        _page.ResumeLayout();
    }

    private static void SelectValue<T>(
        ThemedComboBox combo,
        IEnumerable<T> items,
        Func<T, string> selector,
        string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return;

        foreach (T item in items)
        {
            if (!string.Equals(selector(item), value, StringComparison.OrdinalIgnoreCase))
                continue;

            combo.SelectedItem = item;
            return;
        }

        // The stored value is not part of the catalog (an unknown language, a
        // printer that is not installed any more, ...). Clearing the selection
        // keeps it visible as "not available" instead of silently replacing it
        // with the first entry of the list when the page is saved.
        combo.SelectedIndex = -1;
    }

    private static string FormatTimeZone(TimeZoneInfo zone)
    {
        TimeSpan offset = zone.GetUtcOffset(DateTime.Now);
        string sign = offset < TimeSpan.Zero ? "-" : "+";
        offset = offset.Duration();
        return $"{zone.Id} (UTC{sign}{offset.Hours:00}:{offset.Minutes:00})";
    }

    // ── Items ──────────────────────────────────────────────────────────────

    private sealed record LanguageItem(string Code, string Display);

    private sealed record CountryItem(string Code, string Display);

    private sealed record TimeZoneItem(string Id, string Display);

    private sealed record CurrencyItem(string Code, string Display, string Name);

    private sealed record PrinterItem(string Name, string Display);

    private sealed record PaperItem(string Name, string Display);

    private sealed record OrientationItem(string Code, string Display);

    private sealed record ReceiptItem(string Value, string Display);

    private sealed record CameraItem(string Moniker, string Display);
}
