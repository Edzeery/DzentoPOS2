using DzentoPOS2.App.Services.Inventory;
using DzentoPOS2.Core.Entities.Inventory;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Localization.Resources;
using DzentoPOS2.WinForms.Themes;
using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.WinForms.Forms.Inventory;

public class ProductVariantsDialog : Form
{
    private readonly AppDbContext _context;
    private readonly Guid _productId;
    private readonly DataGridView _grid = new() { Dock = DockStyle.Top, Height = 220, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
    private readonly TextBox _txtName = new() { Width = 120 };
    private readonly TextBox _txtSku = new() { Width = 100 };
    private readonly NumericUpDown _numPrice = new() { Width = 90, DecimalPlaces = 2, Maximum = 999999 };
    private readonly NumericUpDown _numCost = new() { Width = 90, DecimalPlaces = 2, Maximum = 999999 };
    private readonly Button _btnAdd = new() { Text = "إضافة", Width = 70 };
    private readonly Button _btnDelete = new() { Text = "حذف", Width = 70 };
    private readonly Button _btnClose = new() { Text = "إغلاق", Width = 70, DialogResult = DialogResult.OK };

    public ProductVariantsDialog(AppDbContext context, Guid productId)
    {
        _context = context;
        _productId = productId;
        Text = "المتغيرات";
        Size = new Size(640, 360);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = AppColors.Surface;
        ForeColor = AppColors.Ink;
        var panel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, Padding = new Padding(10) };
        _grid.Columns.Add("Name", "الاسم");
        _grid.Columns.Add("SKU", "SKU");
        _grid.Columns.Add("Price", "السعر");
        _grid.Columns.Add("Cost", "التكلفة");
        var row = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, AutoSize = true };
        row.Controls.Add(new Label { Text = "الاسم:", AutoSize = true, ForeColor = AppColors.Ink });
        row.Controls.Add(_txtName);
        row.Controls.Add(new Label { Text = "SKU:", AutoSize = true, ForeColor = AppColors.Ink });
        row.Controls.Add(_txtSku);
        row.Controls.Add(new Label { Text = "سعر:", AutoSize = true, ForeColor = AppColors.Ink });
        row.Controls.Add(_numPrice);
        row.Controls.Add(new Label { Text = "تكلفة:", AutoSize = true, ForeColor = AppColors.Ink });
        row.Controls.Add(_numCost);
        row.Controls.Add(_btnAdd);
        var row2 = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, AutoSize = true };
        row2.Controls.Add(_btnDelete);
        row2.Controls.Add(_btnClose);
        panel.Controls.Add(_grid);
        panel.Controls.Add(row);
        panel.Controls.Add(row2);
        Controls.Add(panel);
        _btnAdd.Click += async (_, _) => await AddAsync();
        _btnDelete.Click += async (_, _) => await DeleteAsync();
        _btnClose.Click += (_, _) => Close();
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        _grid.Rows.Clear();
        var variants = await _context.ProductVariants.AsNoTracking().Where(v => v.ProductId == _productId).ToListAsync();
        foreach (var v in variants)
        {
            _grid.Rows.Add(v.Name, v.SKU, v.SalePrice?.Amount ?? 0m, v.CostPrice?.Amount ?? 0m);
        }
    }

    private async Task AddAsync()
    {
        var product = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == _productId);
        var v = new ProductVariant
        {
            ProductId = _productId,
            Name = string.IsNullOrWhiteSpace(_txtName.Text) ? "Variant" : _txtName.Text.Trim(),
            SKU = string.IsNullOrWhiteSpace(_txtSku.Text) ? null : _txtSku.Text.Trim(),
            SalePrice = new Core.ValueObjects.Common.Money(_numPrice.Value, product?.CostPrice?.Currency ?? "DZD"),
            CostPrice = new Core.ValueObjects.Common.Money(_numCost.Value, product?.CostPrice?.Currency ?? "DZD"),
            MinimumStock = new Core.ValueObjects.Inventory.Quantity(0),
            TrackStock = true,
            IsActive = true,
            IsDefault = false
        };
        await _context.ProductVariants.AddAsync(v);
        await _context.SaveChangesAsync();
        _txtName.Clear();
        _txtSku.Clear();
        await LoadAsync();
    }

    private async Task DeleteAsync()
    {
        if (_grid.CurrentRow == null) return;
        // find variant by name+sku? simplify: delete last added non-default
        var name = _grid.CurrentRow.Cells[0].Value?.ToString();
        var v = await _context.ProductVariants.FirstOrDefaultAsync(x => x.ProductId == _productId && x.Name == name && !x.IsDefault);
        if (v != null)
        {
            _context.ProductVariants.Remove(v);
            await _context.SaveChangesAsync();
            await LoadAsync();
        }
    }
}