using DzentoPOS2.WinForms.Modules.Dashboard;
using DzentoPOS2.WinForms.Themes;

namespace DzentoPOS2.WinForms.Forms.Settings;

/// <summary>A labelled field placed inside a settings card.</summary>
/// <param name="Key">Identifier used to refresh the label after a language change.</param>
/// <param name="Label">Caption of the field.</param>
/// <param name="Control">Input control of the field.</param>
internal readonly record struct SettingsField(string Key, string Label, Control Control);

/// <summary>
/// Card used by the settings page. It holds a title, an optional hint and a
/// vertical stack of field rows. The card grows with its content so the page
/// stays correct at any window size.
/// </summary>
internal sealed class SettingsCard : Panel
{
    private const int RowHeight = 46;
    private const int LabelWidth = 190;

    private readonly TableLayoutPanel _stack;
    private readonly Label _titleLabel;
    private readonly Label _hintLabel;
    private readonly Dictionary<string, Label> _fieldLabels = new();

    public SettingsCard(string title, string hint)
    {
        BackColor = AppColors.Surface;
        Padding = new Padding(20, 12, 20, 16);
        Margin = new Padding(0, 0, 0, 16);
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;

        _titleLabel = new Label
        {
            Text = title,
            Font = AppStyle.SubtitleFont,
            ForeColor = AppColors.Ink,
            BackColor = Color.Transparent,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Margin = new Padding(0, 0, 0, 2)
        };

        _hintLabel = new Label
        {
            Text = hint,
            Font = new Font("Segoe UI", 9F),
            ForeColor = AppColors.InkMuted,
            BackColor = Color.Transparent,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Margin = new Padding(0, 0, 0, 6)
        };

        _stack = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 1,
            RowCount = 0,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            BackColor = Color.Transparent,
            GrowStyle = TableLayoutPanelGrowStyle.AddRows
        };

        _stack.Controls.Add(_titleLabel, 0, 0);
        _stack.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _stack.Controls.Add(_hintLabel, 0, 1);
        _stack.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        Controls.Add(_stack);

        UIHelper.SetRoundedRegion(this, AppStyle.RadiusMedium);
    }

    /// <summary>Title of the card.</summary>
    public string Title
    {
        get => _titleLabel.Text;
        set => _titleLabel.Text = value;
    }

    /// <summary>Hint shown under the title.</summary>
    public string Hint
    {
        get => _hintLabel.Text;
        set => _hintLabel.Text = value;
    }

    /// <summary>Adds a row with one or more fields side by side.</summary>
    public void AddFields(params SettingsField[] fields)
    {
        if (fields.Length == 0) return;

        var row = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = fields.Length * 2,
            RowCount = 1,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            BackColor = Color.Transparent,
            Margin = new Padding(0, 0, 0, 8)
        };

        row.RowStyles.Add(new RowStyle(SizeType.Absolute, RowHeight));

        foreach (SettingsField field in fields)
        {
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, LabelWidth));
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / fields.Length));
        }

        for (int i = 0; i < fields.Length; i++)
        {
            SettingsField field = fields[i];

            var label = new Label
            {
                Text = field.Label,
                Font = AppStyle.BodyFont,
                ForeColor = AppColors.Ink,
                BackColor = Color.Transparent,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(0, 0, 12, 0)
            };

            field.Control.Dock = DockStyle.Fill;
            field.Control.Margin = new Padding(0, 6, 16, 6);

            row.Controls.Add(label, i * 2, 0);
            row.Controls.Add(field.Control, i * 2 + 1, 0);

            _fieldLabels[field.Key] = label;
        }

        int rowIndex = _stack.RowCount++;
        _stack.Controls.Add(row, 0, rowIndex);
        _stack.RowStyles.Add(new RowStyle(SizeType.AutoSize));
    }

    /// <summary>Adds a control that spans the whole card width.</summary>
    public void AddControl(Control control)
    {
        control.Dock = DockStyle.Fill;
        control.Margin = new Padding(0, 0, 0, 8);

        int rowIndex = _stack.RowCount++;
        _stack.Controls.Add(control, 0, rowIndex);
        _stack.RowStyles.Add(new RowStyle(SizeType.AutoSize));
    }

    /// <summary>Refreshes the caption of a field after a language change.</summary>
    public void SetFieldLabel(string key, string text)
    {
        if (_fieldLabels.TryGetValue(key, out Label? label))
            label.Text = text;
    }
}
