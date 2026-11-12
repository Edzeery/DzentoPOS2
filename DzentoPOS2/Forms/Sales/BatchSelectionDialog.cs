using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DzentoPOS2.App.Services.Inventory;
using DzentoPOS2.Core.Entities.Inventory;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Localization.Resources;
using DzentoPOS2.WinForms.Helpers;
using DzentoPOS2.WinForms.Themes;
using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.WinForms.Forms.Sales;

public class BatchSelectionDialog : ThemedDialog
{
    private readonly AppDbContext _context;
    private readonly Guid _variantId;
    private readonly Guid _warehouseId;
    private readonly decimal _requiredQty;

    private readonly ListBox _lst = new() { Dock = DockStyle.Fill, IntegralHeight = false };
    private readonly Button _btnSelect = new() { Text = AppStrings.Select, AutoSize = true, MinimumSize = new Size(80, 32) };
    private readonly Button _btnClose = new() { Text = AppStrings.Close, AutoSize = true, MinimumSize = new Size(80, 32), DialogResult = DialogResult.Cancel };

    private readonly List<Batch> _batches = new();
    private Guid _selectedBatchId;

    public Guid SelectedBatchId => _selectedBatchId;
    public string SelectedBatchNumber { get; private set; } = string.Empty;

    public BatchSelectionDialog(AppDbContext context, Guid variantId, Guid warehouseId, decimal requiredQty)
    {
        _context = context;
        _variantId = variantId;
        _warehouseId = warehouseId;
        _requiredQty = requiredQty;

        Text = AppStrings.SelectBatch;
        Size = new Size(480, 360);
        MinimumSize = new Size(360, 260);
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
            if (_lst.SelectedItem is BatchItem item)
            {
                _selectedBatchId = item.BatchId;
                SelectedBatchNumber = item.BatchNumber;
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
        var batches = await _context.Batches
            .AsNoTracking()
            .Where(b => b.VariantId == _variantId && b.WarehouseId == _warehouseId && b.Quantity > 0)
            .OrderBy(b => b.ExpiryDate ?? DateTime.MaxValue)
            .ThenBy(b => b.ReceivedAt)
            .ToListAsync();

        _lst.Items.Clear();
        foreach (var b in batches)
        {
            var expiry = b.ExpiryDate.HasValue
                ? b.ExpiryDate.Value.ToString("yyyy-MM-dd")
                : AppStrings.NoExpiry;

            _lst.Items.Add(new BatchItem
            {
                BatchId = b.Id,
                BatchNumber = b.BatchNumber,
                Display = $"{b.BatchNumber} | {AppStrings.ExpiryDate}: {expiry} | {AppStrings.Quantity}: {b.Quantity}"
            });
        }
    }

    private sealed class BatchItem
    {
        public Guid BatchId { get; set; }
        public string BatchNumber { get; set; } = "";
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
