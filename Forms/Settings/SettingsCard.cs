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
/// vertical stack of field rows.
/// <para>
/// The width of the card is owned by the grid of the page, the height is
/// computed from the content. This keeps the card independent from the size
/// of its controls, so a long caption can never stretch the card.
/// </para>
/// </summary>
internal sealed class SettingsCard : Panel
{
    private const int RowHeight = 46;
    private const int StackedLabelHeight = 22;
    private const int StackedInputHeight = 36;
    private const int RowGap = 8;
    private const int LabelWidth = 190;
    private const int StackedLabelWidth = 140;

    private readonly TableLayoutPanel _stack;
    private readonly Label _titleLabel;
    private readonly Label _hintLabel;
    private readonly Dictionary<string, Label> _fieldLabels = new();
    private readonly List<FieldRow> _fieldRows = new();
    private readonly List<ControlRow> _controlRows = new();

    private bool _stacked;
    private int _innerWidth = -1;
    private int _nextRow = 2;

    /// <summary>A row that holds one or more fields side by side.</summary>
    private sealed class FieldRow
    {
        public SettingsField[] Fields { get; init; } = Array.Empty<SettingsField>();
        public TableLayoutPanel Panel { get; init; } = new();
        public int RowIndex { get; init; }
        public int Height { get; set; } = RowHeight;
    }

    /// <summary>A row that holds one control spanning the whole card.</summary>
    private sealed class ControlRow
    {
        public Control Control { get; init; } = new();
        public int RowIndex { get; init; }
        public int Height { get; init; }
    }

    public SettingsCard(string title, string hint)
    {
        BackColor = AppColors.Surface;
        Padding = new Padding(20, 12, 20, 16);
        Margin = Padding.Empty;
        AutoSize = false;

        _titleLabel = new Label
        {
            Text = title,
            Font = AppStyle.SubtitleFont,
            ForeColor = AppColors.Ink,
            BackColor = Color.Transparent,
            Dock = DockStyle.Fill,
            AutoSize = true,
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
            AutoSize = true,
            TextAlign = ContentAlignment.MiddleLeft,
            Margin = new Padding(0, 0, 0, 6)
        };

        _stack = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 0,
            AutoSize = false,
            BackColor = Color.Transparent,
            GrowStyle = TableLayoutPanelGrowStyle.AddRows,
            Margin = Padding.Empty
        };
        _stack.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

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
            Dock = DockStyle.Top,
            Height = RowHeight,
            AutoSize = false,
            BackColor = Color.Transparent,
            Margin = Padding.Empty
        };

        int rowIndex = _nextRow++;
        var fieldRow = new FieldRow { Fields = fields, Panel = row, RowIndex = rowIndex };
        _fieldRows.Add(fieldRow);

        BuildFieldRow(fieldRow);

        _stack.Controls.Add(row, 0, rowIndex);
        _stack.RowStyles.Add(new RowStyle(SizeType.Absolute, fieldRow.Height + RowGap));
    }

    /// <summary>Adds a control that spans the whole card width.</summary>
    /// <param name="control">Control added to the card.</param>
    /// <param name="height">Height of the row. Zero keeps the height of the control.</param>
    public void AddControl(Control control, int height = 0)
    {
        control.Dock = DockStyle.Fill;
        control.Margin = new Padding(0, 0, 0, 8);

        int rowIndex = _nextRow++;
        _stack.Controls.Add(control, 0, rowIndex);
        _stack.RowStyles.Add(new RowStyle(SizeType.Absolute, height > 0 ? height + 8 : 40));

        _controlRows.Add(new ControlRow
        {
            Control = control,
            RowIndex = rowIndex,
            Height = height
        });
    }

    /// <summary>Refreshes the caption of a field after a language change.</summary>
    public void SetFieldLabel(string key, string text)
    {
        if (_fieldLabels.TryGetValue(key, out Label? label))
            label.Text = text;
    }

    /// <summary>
    /// Adapts the card to the width it was given by the page and returns the
    /// height the content needs.
    /// </summary>
    public int ApplyWidth(int innerWidth)
    {
        innerWidth = Math.Max(120, innerWidth);

        bool widthChanged = _innerWidth != innerWidth;
        if (widthChanged)
        {
            _innerWidth = innerWidth;
            _stacked = innerWidth < 560;

            // The title and the hint wrap inside the available width.
            _titleLabel.MaximumSize = new Size(innerWidth, 0);
            _hintLabel.MaximumSize = new Size(innerWidth, 0);
        }

        // A width change needs the labels to be re-created (their measured
        // width changed); a text change only needs the heights to be measured
        // again. Both happen when the language changes.
        RefreshRows(rebuild: widthChanged);

        return RequiredHeight();
    }

    /// <summary>Height needed by the content of the card.</summary>
    public int RequiredHeight()
    {
        // Every row is absolute after RefreshRows, so the real height of the
        // stack is the sum of its rows: relying on GetPreferredSize left the
        // wrapped captions a couple of pixels short and clipped the last field.
        int total = 0;
        foreach (RowStyle style in _stack.RowStyles)
            total += (int)Math.Ceiling(style.Height);

        return Padding.Vertical + _stack.Margin.Vertical + total;
    }

    private void RefreshRows(bool rebuild)
    {
        UpdateHeaderStyles();
        UpdateControlRows();

        foreach (FieldRow fieldRow in _fieldRows)
        {
            if (rebuild)
                BuildFieldRow(fieldRow);
            else
                UpdateRowStyles(fieldRow);

            fieldRow.Panel.Height = fieldRow.Height;

            if (fieldRow.RowIndex < _stack.RowStyles.Count)
                _stack.RowStyles[fieldRow.RowIndex] =
                    new RowStyle(SizeType.Absolute, fieldRow.Height + RowGap);
        }
    }

    private void UpdateHeaderStyles()
    {
        int inner = Math.Max(120, _innerWidth);

        int title = TextRenderer.MeasureText(
            _titleLabel.Text, _titleLabel.Font,
            new Size(inner, int.MaxValue), TextFormatFlags.WordBreak).Height;

        int hint = TextRenderer.MeasureText(
            _hintLabel.Text, _hintLabel.Font,
            new Size(inner, int.MaxValue), TextFormatFlags.WordBreak).Height;

        SetRowStyle(_stack, 0, title + _titleLabel.Margin.Vertical + 2);
        SetRowStyle(_stack, 1, hint + _hintLabel.Margin.Vertical + 2);
    }

    private void UpdateControlRows()
    {
        int inner = Math.Max(120, _innerWidth);

        foreach (ControlRow controlRow in _controlRows)
        {
            int height = controlRow.Height > 0
                ? controlRow.Height
                : controlRow.Control.GetPreferredSize(new Size(inner, 0)).Height;

            controlRow.Control.Height = height;
            SetRowStyle(_stack, controlRow.RowIndex, height + 8);
        }
    }

    private void UpdateRowStyles(FieldRow fieldRow)
    {
        fieldRow.Height = MeasureRow(fieldRow);

        if (_stacked)
        {
            for (int i = 0; i < fieldRow.Fields.Length; i++)
            {
                SetRowStyle(fieldRow.Panel, i * 2,
                    LabelHeight(fieldRow.Fields[i], StackedLabelWidth));
                SetRowStyle(fieldRow.Panel, i * 2 + 1, StackedInputHeight);
            }
        }
        else
        {
            SetRowStyle(fieldRow.Panel, 0, fieldRow.Height);
        }
    }

    private static void SetRowStyle(TableLayoutPanel panel, int index, int height)
    {
        if (index < panel.RowStyles.Count)
            panel.RowStyles[index] = new RowStyle(SizeType.Absolute, height);
    }

    private int MeasureRow(FieldRow fieldRow)
    {
        if (_stacked)
        {
            int height = 0;
            foreach (SettingsField field in fieldRow.Fields)
                height += LabelHeight(field, StackedLabelWidth) + StackedInputHeight;
            return height;
        }

        int rowHeight = RowHeight;
        foreach (SettingsField field in fieldRow.Fields)
            rowHeight = Math.Max(rowHeight, LabelHeight(field, LabelWidth - 12));

        return rowHeight;
    }

    /// <summary>
    /// Height the caption needs at the given width. The measurement uses the
    /// live text (it changes with the language) and the font of the real label.
    /// </summary>
    private int LabelHeight(SettingsField field, int width)
    {
        _fieldLabels.TryGetValue(field.Key, out Label? label);
        string text = label?.Text ?? field.Label;
        Font font = label?.Font ?? AppStyle.BodyFont;

        Size measured = TextRenderer.MeasureText(
            text, font,
            new Size(Math.Max(40, width), int.MaxValue),
            TextFormatFlags.WordBreak);

        return Math.Max(StackedLabelHeight, measured.Height + 2);
    }

    private void BuildFieldRow(FieldRow fieldRow)
    {
        TableLayoutPanel row = fieldRow.Panel;
        row.SuspendLayout();
        row.Controls.Clear();
        row.ColumnStyles.Clear();
        row.RowStyles.Clear();

        if (_stacked)
        {
            // Narrow card: every field gets its own line, the caption above
            // the input. The caption row grows when the text wraps.
            row.ColumnCount = 1;
            row.RowCount = fieldRow.Fields.Length * 2;
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            for (int i = 0; i < fieldRow.Fields.Length; i++)
            {
                SettingsField field = fieldRow.Fields[i];

                Label label = MakeLabel(field, StackedLabelWidth);
                row.Controls.Add(label, 0, i * 2);
                row.RowStyles.Add(new RowStyle(
                    SizeType.Absolute, LabelHeight(field, StackedLabelWidth)));

                field.Control.Dock = DockStyle.Fill;
                field.Control.Margin = Padding.Empty;
                row.Controls.Add(field.Control, 0, i * 2 + 1);
                row.RowStyles.Add(new RowStyle(SizeType.Absolute, StackedInputHeight));
            }
        }
        else
        {
            // Wide card: the caption and the input share the line.
            row.ColumnCount = fieldRow.Fields.Length * 2;
            row.RowCount = 1;

            for (int i = 0; i < fieldRow.Fields.Length; i++)
            {
                SettingsField field = fieldRow.Fields[i];

                row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, LabelWidth));
                row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / fieldRow.Fields.Length));

                Label label = MakeLabel(field, LabelWidth - 12);
                row.Controls.Add(label, i * 2, 0);

                field.Control.Dock = DockStyle.Fill;
                field.Control.Margin = new Padding(0, 6, 16, 6);
                row.Controls.Add(field.Control, i * 2 + 1, 0);
            }
        }

        fieldRow.Height = MeasureRow(fieldRow);
        row.Height = fieldRow.Height;
        if (!_stacked)
            row.RowStyles.Add(new RowStyle(SizeType.Absolute, fieldRow.Height));

        row.ResumeLayout();
    }

    private Label MakeLabel(SettingsField field, int width)
    {
        if (!_fieldLabels.TryGetValue(field.Key, out Label? label))
        {
            label = new Label
            {
                Font = AppStyle.BodyFont,
                ForeColor = AppColors.Ink,
                BackColor = Color.Transparent,
                Dock = DockStyle.Fill,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft
            };
            _fieldLabels[field.Key] = label;
        }

        label.Text = field.Label;
        label.Margin = _stacked ? new Padding(0, 0, 0, 2) : new Padding(0, 0, 12, 0);
        label.MaximumSize = new Size(width, 0);
        return label;
    }
}


