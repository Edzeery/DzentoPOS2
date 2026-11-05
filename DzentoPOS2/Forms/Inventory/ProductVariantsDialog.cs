using DzentoPOS2.App.Services.Inventory;
using DzentoPOS2.Core.Entities.Inventory;
using DzentoPOS2.Core.Services.Inventory;
using DzentoPOS2.Core.ValueObjects.Common;
using DzentoPOS2.Core.ValueObjects.Inventory;
using DzentoPOS2.Data.Context;
using DzentoPOS2.WinForms.Helpers;
using DzentoPOS2.WinForms.Themes;
using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.WinForms.Forms.Inventory;

/// <summary>
/// Variant editor for a product. Two tabs: the raw variant list (manual
/// add/delete) and the product's attributes/values, from which all value
/// combinations can be generated automatically into variants.
/// </summary>
public class ProductVariantsDialog : ThemedDialog
{
    private readonly AppDbContext _context;
    private readonly Guid _productId;

    // Variants tab
    private readonly DataGridView _grid = new() { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
    private readonly TextBox _txtName = new() { Width = 110 };
    private readonly TextBox _txtSku = new() { Width = 90 };
    private readonly NumericUpDown _numPrice = new() { Width = 90, DecimalPlaces = 2, Maximum = 999999 };
    private readonly NumericUpDown _numCost = new() { Width = 90, DecimalPlaces = 2, Maximum = 999999 };
    private readonly Button _btnAdd = new() { Text = "إضافة", Width = 70 };
    private readonly Button _btnDelete = new() { Text = "حذف", Width = 70 };

    // Attributes tab
    private readonly DataGridView _gridAttrs = new() { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
    private readonly TextBox _txtAttrName = new() { Width = 110 };
    private readonly TextBox _txtAttrValues = new() { Width = 200 };
    private readonly Button _btnAddAttr = new() { Text = "إضافة خاصية", Width = 100 };
    private readonly Button _btnDeleteAttr = new() { Text = "حذف خاصية", Width = 90 };
    private readonly Button _btnGenerate = new() { Text = "توليد التوليفات", Width = 120 };

    private readonly Button _btnClose = new() { Text = "إغلاق", Width = 70, DialogResult = DialogResult.OK };

    public ProductVariantsDialog(AppDbContext context, Guid productId)
    {
        _context = context;
        _productId = productId;
        Text = "المتغيرات";
        Size = new Size(680, 460);
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(560, 380);

        var tabs = new TabControl { Dock = DockStyle.Fill };

        // ---- Variants tab ----
        var tabVariants = new TabPage("المتغيرات");
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", Visible = false });
        _grid.Columns.Add("Name", "الاسم");
        _grid.Columns.Add("SKU", "SKU");
        _grid.Columns.Add("Price", "السعر");
        _grid.Columns.Add("Cost", "التكلفة");

        var addRow = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 42, FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(6), WrapContents = false };
        addRow.Controls.Add(new Label { Text = "الاسم:", AutoSize = true, Margin = new Padding(3, 8, 3, 3) });
        addRow.Controls.Add(_txtName);
        addRow.Controls.Add(new Label { Text = "SKU:", AutoSize = true, Margin = new Padding(3, 8, 3, 3) });
        addRow.Controls.Add(_txtSku);
        addRow.Controls.Add(new Label { Text = "سعر:", AutoSize = true, Margin = new Padding(3, 8, 3, 3) });
        addRow.Controls.Add(_numPrice);
        addRow.Controls.Add(new Label { Text = "تكلفة:", AutoSize = true, Margin = new Padding(3, 8, 3, 3) });
        addRow.Controls.Add(_numCost);
        addRow.Controls.Add(_btnAdd);

        tabVariants.Controls.Add(_grid);
        tabVariants.Controls.Add(addRow);

        // ---- Attributes tab ----
        var tabAttrs = new TabPage("الخصائص");
        _gridAttrs.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", Visible = false });
        _gridAttrs.Columns.Add("Name", "الخاصية");
        _gridAttrs.Columns.Add("Values", "القيم");

        var attrRow = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 42, FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(6), WrapContents = false };
        attrRow.Controls.Add(new Label { Text = "الخاصية:", AutoSize = true, Margin = new Padding(3, 8, 3, 3) });
        attrRow.Controls.Add(_txtAttrName);
        attrRow.Controls.Add(new Label { Text = "القيم (بفاصلة):", AutoSize = true, Margin = new Padding(3, 8, 3, 3) });
        attrRow.Controls.Add(_txtAttrValues);
        attrRow.Controls.Add(_btnAddAttr);

        var attrRow2 = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 42, FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(6), WrapContents = false };
        attrRow2.Controls.Add(_btnDeleteAttr);
        attrRow2.Controls.Add(_btnGenerate);

        tabAttrs.Controls.Add(_gridAttrs);
        tabAttrs.Controls.Add(attrRow);
        tabAttrs.Controls.Add(attrRow2);

        tabs.TabPages.Add(tabVariants);
        tabs.TabPages.Add(tabAttrs);

        var footer = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 46, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(6) };
        footer.Controls.Add(_btnClose);

        Controls.Add(tabs);
        Controls.Add(footer);

        _btnAdd.Click += async (_, _) => await AddVariantAsync();
        _btnDelete.Click += async (_, _) => await DeleteVariantAsync();
        _btnAddAttr.Click += async (_, _) => await AddAttributeAsync();
        _btnDeleteAttr.Click += async (_, _) => await DeleteAttributeAsync();
        _btnGenerate.Click += async (_, _) => await GenerateCombinationsAsync();
        _btnClose.Click += (_, _) => Close();

        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        _grid.Rows.Clear();
        var variants = await _context.ProductVariants.AsNoTracking()
            .Where(v => v.ProductId == _productId)
            .OrderByDescending(v => v.IsDefault)
            .ToListAsync();

        foreach (var v in variants)
            _grid.Rows.Add(v.Id, v.Name, v.SKU, v.SalePrice?.Amount ?? 0m, v.CostPrice?.Amount ?? 0m);

        DataGridViewStyleHelper.ApplyModernStyle(_grid, true);
        await LoadAttributesAsync();
    }

    private async Task LoadAttributesAsync()
    {
        _gridAttrs.Rows.Clear();
        var defs = await _context.AttributeDefinitions.AsNoTracking()
            .Include(d => d.Values)
            .Where(d => d.ProductId == _productId)
            .OrderBy(d => d.SortOrder)
            .ToListAsync();

        foreach (var d in defs)
            _gridAttrs.Rows.Add(d.Id, d.Name, string.Join(", ", d.Values.OrderBy(v => v.CreatedAt).Select(v => v.Value)));

        DataGridViewStyleHelper.ApplyModernStyle(_gridAttrs);
    }

    private async Task AddVariantAsync()
    {
        var product = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == _productId);
        var currency = product?.CostPrice?.Currency ?? "DZD";
        var v = new ProductVariant
        {
            ProductId = _productId,
            Name = string.IsNullOrWhiteSpace(_txtName.Text) ? "Variant" : _txtName.Text.Trim(),
            SKU = string.IsNullOrWhiteSpace(_txtSku.Text) ? null : _txtSku.Text.Trim(),
            SalePrice = new Money(_numPrice.Value, currency),
            CostPrice = new Money(_numCost.Value, currency),
            MinimumStock = new Quantity(0),
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

    private async Task DeleteVariantAsync()
    {
        if (_grid.CurrentRow?.Cells["Id"].Value is not Guid id) return;

        var v = await _context.ProductVariants.FirstOrDefaultAsync(x => x.Id == id);
        if (v == null || v.IsDefault) return;

        var hasStock = await _context.ProductStocks.AnyAsync(s => s.VariantId == id && s.Quantity.Value != 0);
        if (hasStock)
        {
            MessageBox.Show("لا يمكن حذف متغيّر عليه مخزون.", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var links = await _context.VariantAttributes.Where(a => a.VariantId == id).ToListAsync();
        _context.VariantAttributes.RemoveRange(links);
        _context.ProductVariants.Remove(v);
        await _context.SaveChangesAsync();
        await LoadAsync();
    }

    private async Task AddAttributeAsync()
    {
        var name = _txtAttrName.Text.Trim();
        if (string.IsNullOrWhiteSpace(name)) return;

        var values = _txtAttrValues.Text
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (values.Count == 0) return;

        var maxOrder = await _context.AttributeDefinitions
            .Where(d => d.ProductId == _productId)
            .Select(d => (int?)d.SortOrder)
            .MaxAsync() ?? -1;

        var def = new AttributeDefinition { ProductId = _productId, Name = name, SortOrder = maxOrder + 1 };
        await _context.AttributeDefinitions.AddAsync(def);
        await _context.SaveChangesAsync();

        foreach (var value in values)
            await _context.AttributeValues.AddAsync(new AttributeValue { DefinitionId = def.Id, Value = value });
        await _context.SaveChangesAsync();

        _txtAttrName.Clear();
        _txtAttrValues.Clear();
        await LoadAttributesAsync();
    }

    private async Task DeleteAttributeAsync()
    {
        if (_gridAttrs.CurrentRow?.Cells["Id"].Value is not Guid id) return;

        var def = await _context.AttributeDefinitions.Include(d => d.Values).FirstOrDefaultAsync(d => d.Id == id);
        if (def == null) return;

        var valueIds = def.Values.Select(v => v.Id).ToList();
        var links = await _context.VariantAttributes
            .Where(a => a.DefinitionId == id || valueIds.Contains(a.ValueId))
            .ToListAsync();

        _context.VariantAttributes.RemoveRange(links);
        _context.AttributeValues.RemoveRange(def.Values);
        _context.AttributeDefinitions.Remove(def);
        await _context.SaveChangesAsync();
        await LoadAttributesAsync();
    }

    private async Task GenerateCombinationsAsync()
    {
        var defs = await _context.AttributeDefinitions
            .Include(d => d.Values)
            .Where(d => d.ProductId == _productId)
            .OrderBy(d => d.SortOrder)
            .ToListAsync();

        if (defs.Count == 0)
        {
            MessageBox.Show("أضف خاصية واحدة على الأقل قبل التوليد.", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (defs.Any(d => d.Values.Count == 0))
        {
            MessageBox.Show("كل خاصية يجب أن تحتوي على قيمة واحدة على الأقل.", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var product = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == _productId);
        var currency = product?.CostPrice?.Currency ?? "DZD";
        var cost = product?.CostPrice?.Amount ?? 0m;
        var price = product?.SalePrice?.Amount ?? 0m;

        // Cartesian product of the attribute values.
        var axes = defs
            .Select(d => (IReadOnlyList<AttributeValue>)d.Values.OrderBy(v => v.CreatedAt).ToList())
            .ToList();
        var combinations = VariantCombinationGenerator.Generate(axes);

        // Signature of every existing variant so re-running is idempotent.
        var existing = await _context.VariantAttributes
            .AsNoTracking()
            .Where(a => a.Variant.ProductId == _productId)
            .Select(a => new { a.VariantId, a.ValueId })
            .ToListAsync();

        var existingKeys = existing
            .GroupBy(x => x.VariantId)
            .Select(g => string.Join("|", g.Select(x => x.ValueId).OrderBy(x => x)))
            .ToHashSet(StringComparer.Ordinal);

        int created = 0;
        foreach (var combo in combinations)
        {
            var key = string.Join("|", combo.Select(v => v.Id).OrderBy(x => x));
            if (!existingKeys.Add(key)) continue;

            var variant = new ProductVariant
            {
                ProductId = _productId,
                Name = string.Join(" / ", combo.Select(v => v.Value)),
                CostPrice = new Money(cost, currency),
                SalePrice = new Money(price, currency),
                MinimumStock = new Quantity(0),
                TrackStock = product?.TrackStock ?? true,
                IsActive = true,
                IsDefault = false
            };
            await _context.ProductVariants.AddAsync(variant);
            await _context.SaveChangesAsync();

            foreach (var value in combo)
            {
                await _context.VariantAttributes.AddAsync(new VariantAttribute
                {
                    VariantId = variant.Id,
                    DefinitionId = value.DefinitionId,
                    ValueId = value.Id
                });
            }
            await _context.SaveChangesAsync();
            created++;
        }

        await LoadAsync();
        MessageBox.Show(
            created == 0 ? "لا توجد توليفات جديدة (كلها موجودة)." : $"تم إنشاء {created} متغيّر.",
            "توليد التوليفات",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    protected override void ApplyTheme()
    {
        base.ApplyTheme();
        _btnGenerate.BackColor = AppColors.Primary;
        _btnGenerate.ForeColor = AppColors.OnDark;
        _btnClose.BackColor = AppColors.Neutral;
        _btnClose.ForeColor = AppColors.OnDark;
    }
}
