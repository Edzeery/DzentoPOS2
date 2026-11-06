using DzentoPOS2.Localization.Resources;
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
    private readonly TextBox _txtName = new() { Dock = DockStyle.Fill, MinimumSize = new Size(0, 28) };
    private readonly TextBox _txtSku = new() { Dock = DockStyle.Fill, MinimumSize = new Size(0, 28) };
    private readonly NumericUpDown _numPrice = new() { Dock = DockStyle.Fill, DecimalPlaces = 2, Maximum = 999999, MinimumSize = new Size(0, 28) };
    private readonly NumericUpDown _numCost = new() { Dock = DockStyle.Fill, DecimalPlaces = 2, Maximum = 999999, MinimumSize = new Size(0, 28) };
    private readonly Button _btnAdd = new() { Text = AppStrings.Add, AutoSize = true, MinimumSize = new Size(80, 32) };
    private readonly Button _btnDelete = new() { Text = AppStrings.Delete, AutoSize = true, MinimumSize = new Size(70, 32) };

    // Attributes tab
    private readonly DataGridView _gridAttrs = new() { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
    private readonly TextBox _txtAttrName = new() { Dock = DockStyle.Fill, MinimumSize = new Size(0, 28) };
    private readonly TextBox _txtAttrValues = new() { Dock = DockStyle.Fill, MinimumSize = new Size(0, 28) };
    private readonly Button _btnAddAttr = new() { Text = AppStrings.AddAttribute, AutoSize = true, MinimumSize = new Size(100, 32) };
    private readonly Button _btnDeleteAttr = new() { Text = AppStrings.DeleteAttribute, AutoSize = true, MinimumSize = new Size(90, 32) };
    private readonly Button _btnGenerate = new() { Text = AppStrings.GenerateCombinations, AutoSize = true, MinimumSize = new Size(120, 32) };

    private readonly Button _btnClose = new() { Text = AppStrings.Close, AutoSize = true, MinimumSize = new Size(70, 32), DialogResult = DialogResult.OK };

    public ProductVariantsDialog(AppDbContext context, Guid productId)
    {
        _context = context;
        _productId = productId;
        Text = AppStrings.VariantsTab;
        Size = new Size(680, 460);
        MinimumSize = new Size(320, 320);
        StartPosition = FormStartPosition.CenterParent;
        AutoScaleMode = AutoScaleMode.Font;

        var tabs = new TabControl { Dock = DockStyle.Fill };

        // ---- Variants tab ----
        var tabVariants = new TabPage(AppStrings.VariantsTab);
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", Visible = false });
        _grid.Columns.Add("Name", AppStrings.Name);
        _grid.Columns.Add("SKU", "SKU");
        _grid.Columns.Add("Price", AppStrings.Price);
        _grid.Columns.Add("Cost", AppStrings.Cost);

        var variantsLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(8),
            Margin = Padding.Empty
        };
        variantsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        variantsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        variantsLayout.Controls.Add(_grid, 0, 0);
        variantsLayout.Controls.Add(BuildVariantEditor(), 0, 1);
        tabVariants.Controls.Add(variantsLayout);

        // ---- Attributes tab ----
        var tabAttrs = new TabPage(AppStrings.Attributes);
        _gridAttrs.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", Visible = false });
        _gridAttrs.Columns.Add("Name", AppStrings.Attribute);
        _gridAttrs.Columns.Add("Values", AppStrings.Values);

        var attrsLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(8),
            Margin = Padding.Empty
        };
        attrsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        attrsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        attrsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        attrsLayout.Controls.Add(_gridAttrs, 0, 0);
        attrsLayout.Controls.Add(BuildAttributeEditor(), 0, 1);
        attrsLayout.Controls.Add(BuildAttributeActions(), 0, 2);
        tabAttrs.Controls.Add(attrsLayout);

        tabs.TabPages.Add(tabVariants);
        tabs.TabPages.Add(tabAttrs);

        var footer = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = true,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Padding = new Padding(8, 4, 8, 8),
            Margin = Padding.Empty
        };
        footer.Controls.Add(_btnClose);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(8),
            Margin = Padding.Empty
        };
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.Controls.Add(tabs, 0, 0);
        root.Controls.Add(footer, 0, 1);
        Controls.Add(root);

        _btnAdd.Click += async (_, _) => await AddVariantAsync();
        _btnDelete.Click += async (_, _) => await DeleteVariantAsync();
        _btnAddAttr.Click += async (_, _) => await AddAttributeAsync();
        _btnDeleteAttr.Click += async (_, _) => await DeleteAttributeAsync();
        _btnGenerate.Click += async (_, _) => await GenerateCombinationsAsync();
        _btnClose.Click += (_, _) => Close();

        _ = LoadAsync();
    }

    /// <summary>
    /// Two columns of label + field, so the editor reflows on a narrow window
    /// instead of overflowing like the old fixed-width flow row.
    /// </summary>
    private Control BuildVariantEditor()
    {
        var fields = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 2,
            RowCount = 2,
            Margin = Padding.Empty
        };
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        fields.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        fields.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        fields.Controls.Add(BuildField(AppStrings.NameLabel, _txtName), 0, 0);
        fields.Controls.Add(BuildField("SKU:", _txtSku), 1, 0);
        fields.Controls.Add(BuildField(AppStrings.PriceLabel, _numPrice), 0, 1);
        fields.Controls.Add(BuildField(AppStrings.CostLabel, _numCost), 1, 1);

        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = true,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = new Padding(0, 6, 0, 0)
        };
        actions.Controls.Add(_btnAdd);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 1,
            RowCount = 2,
            Margin = Padding.Empty
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.Controls.Add(fields, 0, 0);
        root.Controls.Add(actions, 0, 1);
        return root;
    }

    private Control BuildAttributeEditor()
    {
        var fields = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 1,
            RowCount = 2,
            Margin = Padding.Empty
        };
        fields.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        fields.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        fields.Controls.Add(BuildField(AppStrings.AttributeLabel, _txtAttrName), 0, 0);
        fields.Controls.Add(BuildField(AppStrings.ValuesCommaSeparatedLabel, _txtAttrValues), 0, 1);

        var addRow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = true,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = new Padding(0, 6, 0, 0)
        };
        addRow.Controls.Add(_btnAddAttr);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 1,
            RowCount = 2,
            Margin = Padding.Empty
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.Controls.Add(fields, 0, 0);
        root.Controls.Add(addRow, 0, 1);
        return root;
    }

    private Control BuildAttributeActions()
    {
        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = true,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = new Padding(0, 6, 0, 0)
        };
        actions.Controls.Add(_btnDeleteAttr);
        actions.Controls.Add(_btnGenerate);
        return actions;
    }

    private static Control BuildField(string labelText, Control input)
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 2,
            RowCount = 1,
            Margin = new Padding(0, 0, 6, 6)
        };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        panel.Controls.Add(new Label
        {
            Text = labelText,
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Margin = new Padding(0, 6, 6, 0)
        }, 0, 0);
        input.Margin = new Padding(0, 6, 0, 6);
        panel.Controls.Add(input, 1, 0);
        return panel;
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
            MessageBox.Show(AppStrings.VariantHasStockCannotDelete, AppStrings.Warning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            MessageBox.Show(AppStrings.AddAttributeBeforeGenerate, AppStrings.Warning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (defs.Any(d => d.Values.Count == 0))
        {
            MessageBox.Show(AppStrings.AttributeNeedsAtLeastOneValue, AppStrings.Warning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            created == 0 ? AppStrings.NoNewCombinations : string.Format(AppStrings.VariantCreatedFormat, created),
            AppStrings.GenerateCombinations,
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
