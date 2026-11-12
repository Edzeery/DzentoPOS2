using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DzentoPOS2.App.Services.Inventory;
using DzentoPOS2.Core.Entities.Inventory;
using DzentoPOS2.Core.Enums;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Localization.Resources;
using DzentoPOS2.WinForms.Helpers;
using DzentoPOS2.WinForms.Themes;
using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.WinForms.Forms.Sales;

/// <summary>
/// Serial unit picker dialog. Shows available units for a variant/warehouse.
/// Returns the selected unit via <see cref="SelectedUnitId"/>.
/// </summary>
public class SerialUnitPickerDialog : ThemedDialog
{
    private readonly AppDbContext _context;
    private readonly Guid _variantId;
    private readonly Guid _warehouseId;

    private readonly ListBox _lst = new() { Dock = DockStyle.Fill, IntegralHeight = false };
    private readonly Button _btnSelect = new() { Text = AppStrings.Select, AutoSize = true, MinimumSize = new Size(80, 32) };
    private readonly Button _btnClose = new() { Text = AppStrings.Close, AutoSize = true, MinimumSize = new Size(80, 32), DialogResult = DialogResult.Cancel };

    private readonly List<SerialUnit> _units = new();
    private Guid _selectedUnitId;

    public Guid SelectedUnitId => _selectedUnitId;
    public string SelectedSerialNumber { get; private set; } = string.Empty;
    public string SelectedIMEI { get; private set; } = string.Empty;

    public SerialUnitPickerDialog(AppDbContext context, Guid variantId, Guid warehouseId)
    {
        _context = context;
        _variantId = variantId;
        _warehouseId = warehouseId;

        Text = AppStrings.SelectSerialUnit;
        Size = new Size(420, 360);
        MinimumSize = new Size(320, 260);
        StartPosition = FormStartPosition.CenterParent;
        AutoScaleMode = AutoScaleMode.Font;

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(12),
            Margin = Padding.Empty
        };
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        root.Controls.Add(_lst, 0, 0);

        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = true,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = new Padding(0, 8, 0, 0)
        };
        actions.Controls.Add(_btnClose);
        actions.Controls.Add(_btnSelect);
        root.Controls.Add(actions, 0, 2);

        Controls.Add(root);

        _btnSelect.Click += (_, _) =>
        {
            if (_lst.SelectedItem is SerialUnitItem item)
            {
                _selectedUnitId = item.UnitId;
                SelectedSerialNumber = item.SerialNumber;
                SelectedIMEI = item.IMEI;
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MessageBox.Show(AppStrings.PleaseSelectVariant, AppStrings.Warning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        };
        _btnClose.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };

        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        var units = await _context.SerialUnits
            .AsNoTracking()
            .Where(s => s.VariantId == _variantId && s.WarehouseId == _warehouseId && s.Status == SerialUnitStatus.Available)
            .ToListAsync();

        _lst.Items.Clear();
        foreach (var u in units)
        {
            _lst.Items.Add(new SerialUnitItem
            {
                UnitId = u.Id,
                SerialNumber = u.SerialNumber ?? "",
                IMEI = u.Imei1 ?? "",
                Display = $"{u.SerialNumber ?? AppStrings.NoSerial} | {u.Imei1 ?? ""}"
            });
        }
    }

    private class SerialUnitItem
    {
        public Guid UnitId { get; set; }
        public string SerialNumber { get; set; } = "";
        public string IMEI { get; set; } = "";
        public string Display { get; set; } = "";
        public override string ToString() => Display;
    }

    protected override void ApplyTheme()
    {
        base.ApplyTheme();
        _btnSelect.BackColor = AppColors.Primary;
        _btnSelect.ForeColor = AppColors.OnDark;
    }
}
