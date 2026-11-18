using System.Globalization;
using System.Text;
using DzentoPOS2.Licensing;
using DzentoPOS2.Licensing.Signing;

namespace DzentoPOS2.LicenseIssuer;

/// <summary>
/// Vendor-only graphical front end: generate the signing key pair, issue a
/// licence bound to a customer's device fingerprint, and verify a key.
/// </summary>
internal sealed class MainForm : Form
{
    private static readonly (string Plan, int Days, int Users, int Branches)[] Plans =
    [
        ("Basic", 30, 1, 1),
        ("Standard", 365, 3, 1),
        ("Professional", 365, 10, 3),
        ("Enterprise", 365, 999, 99)
    ];

    private static readonly Font MonoFont = new("Consolas", 9F);

    private readonly TabControl _tabs = new() { Dock = DockStyle.Fill };

    // Generate tab
    private readonly TextBox _genPassword = Password();
    private readonly TextBox _genConfirm = Password();
    private readonly ComboBox _genBits = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _genPublic = Mono(multiline: true, readOnly: true);
    private readonly Label _genStatus = Status();

    // Issue tab
    private readonly TextBox _issueKeyPath = new();
    private readonly TextBox _issuePassword = Password();
    private readonly TextBox _issueCustomer = new();
    private readonly TextBox _issueDevice = new();
    private readonly ComboBox _issuePlan = new() { DropDownStyle = ComboBoxStyle.DropDown };
    private readonly NumericUpDown _issueDays = Number(36500, 365);
    private readonly NumericUpDown _issueUsers = Number(100000, 5);
    private readonly NumericUpDown _issueBranches = Number(100000, 2);
    private readonly TextBox _issueSerial = new();
    private readonly TextBox _issueOutput = Mono(multiline: true, readOnly: true);
    private readonly Label _issueStatus = Status();

    // Verify tab
    private readonly TextBox _verifyPublic = Mono(multiline: true, readOnly: false);
    private readonly TextBox _verifyToken = Mono(multiline: true, readOnly: false);
    private readonly TextBox _verifyDevice = new();
    private readonly Label _verifyStatus = Status();

    public MainForm()
    {
        Text = "DzentoPOS2 - Licence Issuer (vendor only)";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(900, 680);
        MinimumSize = new Size(760, 560);
        Font = new Font("Segoe UI", 9F);

        _genBits.Items.AddRange(["2048", "3072", "4096"]);
        _genBits.SelectedIndex = 0;

        _tabs.TabPages.Add(BuildIssueTab());
        _tabs.TabPages.Add(BuildVerifyTab());
        _tabs.TabPages.Add(BuildGenerateTab());
        _tabs.SelectedIndex = 0;

        Controls.Add(_tabs);

        var footer = new Label
        {
            Dock = DockStyle.Bottom,
            Height = 34,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(10, 0, 10, 0),
            Text = "The private key is the only secret. Never ship this tool or the private key to a customer."
        };
        Controls.Add(footer);
    }

    // -- Generate ----------------------------------------------------------

    private TabPage BuildGenerateTab()
    {
        var table = Table();
        var page = new TabPage("3 - Signing keys (setup)") { Padding = new Padding(12), BackColor = Color.White };

        AddRow(table, Label("Password"), _genPassword, 24);
        AddRow(table, Label("Confirm password"), _genConfirm, 24);
        AddRow(table, Label("Key size (bits)"), _genBits, 24);

        var actions = new FlowLayoutPanel { AutoSize = true, WrapContents = false };
        var generate = new Button { Text = "Generate and save...", AutoSize = true };
        generate.Click += OnGenerateClick;
        actions.Controls.Add(generate);
        AddRow(table, Label(string.Empty), actions, 34);

        AddRow(table, Label("Public key"), _genPublic, 180);

        var copy = new FlowLayoutPanel { AutoSize = true, WrapContents = false };
        var copyPublic = new Button { Text = "Copy public key", AutoSize = true };
        copyPublic.Click += (_, _) => CopyToClipboard(_genPublic.Text, _genStatus);
        var copySnippet = new Button { Text = "Copy C# snippet", AutoSize = true };
        copySnippet.Click += (_, _) =>
            CopyToClipboard(BuildCSharpSnippet(_genPublic.Text), _genStatus);
        copy.Controls.Add(copyPublic);
        copy.Controls.Add(copySnippet);
        AddRow(table, Label(string.Empty), copy, 34);

        AddRow(table, Label(string.Empty), _genStatus, 24);

        page.Controls.Add(table);
        return page;
    }

    private void OnGenerateClick(object? sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrEmpty(_genPassword.Text))
            {
                Warn(_genStatus, "Enter a password for the private key.", error: true);
                return;
            }

            if (_genPassword.Text != _genConfirm.Text)
            {
                Warn(_genStatus, "The passwords do not match.", error: true);
                return;
            }

            using var dialog = new SaveFileDialog
            {
                Title = "Save the private key as...",
                FileName = "vendor-private.pem",
                Filter = "PEM key|*.pem|All files|*.*"
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            int bits = int.Parse((string)_genBits.SelectedItem!, CultureInfo.InvariantCulture);

            var (encryptedPrivateKey, publicKey) =
                LicenseSigner.GenerateEncryptedKeyPair(_genPassword.Text, bits);

            File.WriteAllText(dialog.FileName, encryptedPrivateKey, new UTF8Encoding(false));

            var publicPath = Path.ChangeExtension(dialog.FileName, null) + ".pub.pem";
            File.WriteAllText(publicPath, publicKey, new UTF8Encoding(false));

            _genPublic.Text = publicKey;
            _verifyPublic.Text = publicKey;

            Warn(_genStatus,
                $"Saved private key and {Path.GetFileName(publicPath)}.\r\n" +
                "Copy the public key into LicensePublicKey.Pem and rebuild the app.",
                error: false);
        }
        catch (Exception ex)
        {
            Warn(_genStatus, ex.Message, error: true);
        }
    }

    // -- Issue -------------------------------------------------------------

    private TabPage BuildIssueTab()
    {
        var table = Table();
        var page = new TabPage("1 - Issue licence") { Padding = new Padding(12), BackColor = Color.White };

        var keyRow = new TableLayoutPanel
        {
            ColumnCount = 2,
            AutoSize = true,
            Dock = DockStyle.Fill
        };
        keyRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        keyRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        _issueKeyPath.Dock = DockStyle.Fill;
        var browse = new Button { Text = "Browse...", AutoSize = true };
        browse.Click += OnBrowseKeyClick;
        keyRow.Controls.Add(_issueKeyPath, 0, 0);
        keyRow.Controls.Add(browse, 1, 0);
        AddRow(table, Label("Private key file"), keyRow, 26);

        AddRow(table, Label("Password"), _issuePassword, 24);
        AddRow(table, Label("Customer"), _issueCustomer, 24);
        AddRow(table, Label("Device fingerprint"), _issueDevice, 24);

        foreach (var plan in Plans)
            _issuePlan.Items.Add(plan.Plan);
        _issuePlan.Text = Plans[1].Plan;
        _issuePlan.SelectedIndexChanged += OnPlanChanged;
        AddRow(table, Label("Plan"), _issuePlan, 24);

        AddRow(table, Label("Duration (days)"), _issueDays, 24);
        AddRow(table, Label("Max users"), _issueUsers, 24);
        AddRow(table, Label("Max branches"), _issueBranches, 24);
        AddRow(table, Label("Serial (optional)"), _issueSerial, 24);

        var issue = new Button { Text = "Issue licence", AutoSize = true };
        issue.Click += OnIssueClick;
        AddRow(table, Label(string.Empty), issue, 34);

        AddRow(table, Label("Licence key"), _issueOutput, 150);

        var copy = new Button { Text = "Copy licence key", AutoSize = true };
        copy.Click += (_, _) => CopyToClipboard(_issueOutput.Text, _issueStatus);
        AddRow(table, Label(string.Empty), copy, 34);

        AddRow(table, Label(string.Empty), _issueStatus, 24);

        page.Controls.Add(table);
        OnPlanChanged(this, EventArgs.Empty);
        return page;
    }

    private void OnBrowseKeyClick(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Select the private key",
            Filter = "PEM key|*.pem|All files|*.*"
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
            _issueKeyPath.Text = dialog.FileName;
    }

    private void OnPlanChanged(object? sender, EventArgs e)
    {
        var match = Plans.FirstOrDefault(
            p => string.Equals(p.Plan, _issuePlan.Text, StringComparison.OrdinalIgnoreCase));

        if (match.Plan is null)
            return;

        _issueDays.Value = match.Days;
        _issueUsers.Value = match.Users;
        _issueBranches.Value = match.Branches;
    }

    private void OnIssueClick(object? sender, EventArgs e)
    {
        try
        {
            if (!File.Exists(_issueKeyPath.Text))
            {
                Warn(_issueStatus, "Select the private key file.", error: true);
                return;
            }

            if (string.IsNullOrEmpty(_issuePassword.Text) ||
                string.IsNullOrWhiteSpace(_issuePlan.Text) ||
                string.IsNullOrWhiteSpace(_issueDevice.Text))
            {
                Warn(_issueStatus, "Password, plan and device fingerprint are required.", error: true);
                return;
            }

            var now = DateTime.UtcNow;
            var payload = new LicensePayload
            {
                Serial = string.IsNullOrWhiteSpace(_issueSerial.Text)
                    ? now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture)
                    : _issueSerial.Text.Trim(),
                Plan = _issuePlan.Text.Trim(),
                DeviceId = _issueDevice.Text.Trim(),
                LicensedTo = string.IsNullOrWhiteSpace(_issueCustomer.Text)
                    ? null
                    : _issueCustomer.Text.Trim(),
                IssuedAt = now,
                ExpiresAt = now.AddDays((int)_issueDays.Value),
                MaxUsers = (int)_issueUsers.Value,
                MaxBranches = (int)_issueBranches.Value
            };

            var encryptedPrivateKey = File.ReadAllText(_issueKeyPath.Text);
            var token = LicenseSigner.SignEncrypted(
                payload, encryptedPrivateKey, _issuePassword.Text);

            IssuanceLedger.Append(_issueKeyPath.Text, payload, token);

            _issueOutput.Text = token;
            Warn(_issueStatus,
                $"Issued. Ledger: {IssuanceLedger.PathFor(_issueKeyPath.Text)}",
                error: false);
        }
        catch (Exception ex)
        {
            Warn(_issueStatus, $"Failed: {ex.Message}", error: true);
        }
    }

    // -- Verify ------------------------------------------------------------

    private TabPage BuildVerifyTab()
    {
        var table = Table();
        var page = new TabPage("2 - Verify a licence") { Padding = new Padding(12), BackColor = Color.White };

        var load = new Button { Text = "Load public key...", AutoSize = true };
        load.Click += OnLoadPublicClick;
        AddRow(table, Label(string.Empty), load, 34);

        AddRow(table, Label("Public key"), _verifyPublic, 150);
        AddRow(table, Label("Licence key"), _verifyToken, 150);
        AddRow(table, Label("Device (optional)"), _verifyDevice, 24);

        var verify = new Button { Text = "Verify", AutoSize = true };
        verify.Click += OnVerifyClick;
        AddRow(table, Label(string.Empty), verify, 34);

        AddRow(table, Label(string.Empty), _verifyStatus, 24);

        page.Controls.Add(table);
        return page;
    }

    private void OnLoadPublicClick(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Select the public key",
            Filter = "PEM key|*.pem|All files|*.*"
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
            _verifyPublic.Text = File.ReadAllText(dialog.FileName);
    }

    private void OnVerifyClick(object? sender, EventArgs e)
    {
        var publicKey = IssuerCli.ReadPublicKey(_verifyPublic.Text);
        if (string.IsNullOrWhiteSpace(publicKey))
        {
            Warn(_verifyStatus, "Provide the public key.", error: true);
            return;
        }

        var result = LicenseKeyCodec.Verify(_verifyToken.Text.Trim(), publicKey);
        if (!result.IsValid)
        {
            Warn(_verifyStatus, $"INVALID ({result.ErrorCode}).", error: true);
            return;
        }

        var payload = result.Payload!;
        var message = new StringBuilder()
            .AppendLine("VALID")
            .AppendLine($"Plan     : {payload.Plan}")
            .AppendLine($"Serial   : {payload.Serial}")
            .AppendLine($"Customer : {payload.LicensedTo ?? "-"}")
            .AppendLine($"Device   : {payload.DeviceId}")
            .AppendLine($"Expires  : {payload.ExpiresAt:u}")
            .Append($"Limits   : {payload.MaxUsers} user(s), {payload.MaxBranches} branch(es)");

        bool mismatch = !string.IsNullOrWhiteSpace(_verifyDevice.Text) &&
            !string.Equals(payload.DeviceId, _verifyDevice.Text.Trim(), StringComparison.OrdinalIgnoreCase);

        if (mismatch)
            message.AppendLine().Append("WARNING: bound to a different device.");

        Warn(_verifyStatus, message.ToString(), error: mismatch);
    }

    // -- Helpers -----------------------------------------------------------

    private static TableLayoutPanel Table()
    {
        var table = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 0,
            AutoSize = true,
            GrowStyle = TableLayoutPanelGrowStyle.AddRows,
            BackColor = Color.White
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        return table;
    }

    private static void AddRow(TableLayoutPanel table, Control left, Control right, int height)
    {
        int row = table.RowCount++;
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, height));
        table.Controls.Add(left, 0, row);

        right.Dock = DockStyle.Fill;
        table.Controls.Add(right, 1, row);
    }

    private static Label Label(string text) => new()
    {
        Text = text,
        AutoSize = true,
        TextAlign = ContentAlignment.MiddleLeft,
        Margin = new Padding(0, 6, 8, 0)
    };

    private static Label Status() => new()
    {
        AutoSize = true,
        MaximumSize = new Size(700, 0),
        ForeColor = Color.DimGray
    };

    private static TextBox Password() => new()
    {
        UseSystemPasswordChar = true,
        Dock = DockStyle.Fill
    };

    private static TextBox Mono(bool multiline, bool readOnly) => new()
    {
        Multiline = multiline,
        ReadOnly = readOnly,
        Font = MonoFont,
        ScrollBars = ScrollBars.Both,
        WordWrap = false,
        Dock = DockStyle.Fill
    };

    private static NumericUpDown Number(int max, decimal value) => new()
    {
        Maximum = max,
        Minimum = 0,
        Value = value,
        Dock = DockStyle.Fill
    };

    private static void Warn(Label label, string message, bool error)
    {
        label.ForeColor = error ? Color.Firebrick : Color.SeaGreen;
        label.Text = message;
    }

    private void CopyToClipboard(string value, Label status)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            Warn(status, "Nothing to copy.", error: true);
            return;
        }

        try
        {
            Clipboard.SetText(value);
            Warn(status, "Copied.", error: false);
        }
        catch (Exception ex)
        {
            Warn(status, ex.Message, error: true);
        }
    }

    /// <summary>
    /// Renders every tab to a PNG for visual verification. Used by
    /// <c>--screenshot</c>; never runs in normal use.
    /// </summary>
    internal void CaptureScreenshots(string directory)
    {
        Directory.CreateDirectory(directory);

        StartPosition = FormStartPosition.Manual;
        Location = new Point(-4000, -4000);
        Show();
        Application.DoEvents();

        foreach (TabPage page in _tabs.TabPages)
        {
            _tabs.SelectedTab = page;
            PerformLayout();
            Application.DoEvents();

            var name = new string(page.Text
                .Select(c => char.IsLetterOrDigit(c) ? c : '-')
                .ToArray());

            using var bitmap = new Bitmap(page.Width, page.Height);
            page.DrawToBitmap(bitmap, new Rectangle(0, 0, page.Width, page.Height));
            bitmap.Save(
                Path.Combine(directory, $"{name}.png"),
                System.Drawing.Imaging.ImageFormat.Png);
        }

        Close();
    }

    private static string BuildCSharpSnippet(string publicKey)
    {
        var builder = new StringBuilder();
        builder.AppendLine("public const string Pem =");
        builder.AppendLine("    \"\"\"");

        foreach (var line in publicKey
                     .Replace("\r\n", "\n", StringComparison.Ordinal)
                     .TrimEnd('\n')
                     .Split('\n'))
        {
            builder.Append("    ").AppendLine(line);
        }

        builder.AppendLine("    \"\"\";");
        return builder.ToString();
    }
}
