using System;
using System.Collections.Generic;
using System.Drawing;
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

/// <summary>
/// Variant picker dialog. Shows attribute groups (color, size, etc.) as
/// selectable chips and lets the user pick a variant. Returns the selected
/// variant's info via <see cref="SelectedVariantId"/>.
/// </summary>
public class VariantPickerDialog : ThemedDialog
{
    private readonly AppDbContext _context;
    private readonly Guid _productId;
    private readonly IBarcodeService _barcodeService;

    private readonly FlowLayoutPanel _attrPanel = new() { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, WrapContents = true, AutoScroll = true };
    private readonly PictureBox _picPreview = new() { SizeMode = PictureBoxSizeMode.Zoom, Dock = DockStyle.Fill };
    private readonly Label _lblPrice = new() { Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = AppColors.Primary, AutoSize = true };
    private readonly Label _lblBarcode = new() { Font = new Font("Segoe UI", 9F), ForeColor = AppColors.InkMuted, AutoSize = true };
    private readonly Label _lblVariantName = new() { Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = AppColors.Ink, AutoSize = true };
    private readonly Button _btnAdd = new() { Text = AppStrings.AddToCart, AutoSize = true, MinimumSize = new Size(120, 36) };
    private readonly Button _btnClose = new() { Text = AppStrings.Close, AutoSize = true, MinimumSize = new Size(80, 36), DialogResult = DialogResult.Cancel };

    private List<ProductVariant> _variants = new();
    private readonly Dictionary<Guid, List<AttributeValue>> _variantAttrs = new();
    private readonly Dictionary<Guid, Guid> _attrSelection = new(); // definitionId → selected valueId
    private Guid _selectedVariantId;

    public Guid SelectedVariantId => _selectedVariantId;
    public string SelectedVariantName { get; private set; } = string.Empty;
    public decimal SelectedPrice { get; private set; }

    public VariantPickerDialog(AppDbContext context, Guid productId, IBarcodeService barcodeService)
    {
        _context = context;
        _productId = productId;
        _barcodeService = barcodeService;

        Text = AppStrings.SelectVariant;
        Size = new Size(520, 480);
        MinimumSize = new Size(360, 320);
        StartPosition = FormStartPosition.CenterParent;
        AutoScaleMode = AutoScaleMode.Font;

        BuildLayout();
        _ = LoadAsync();
    }

    private void BuildLayout()
    {
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

        // Attributes + preview side by side
        var content = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = Padding.Empty
        };
        content.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));

        content.Controls.Add(_attrPanel, 0, 0);

        var previewPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Margin = new Padding(8, 0, 0, 0)
        };
        previewPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        previewPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        previewPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        previewPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        previewPanel.Controls.Add(_picPreview, 0, 0);
        previewPanel.Controls.Add(_lblVariantName, 0, 1);
        previewPanel.Controls.Add(_lblPrice, 0, 2);
        previewPanel.Controls.Add(_lblBarcode, 0, 3);
        content.Controls.Add(previewPanel, 1, 0);

        root.Controls.Add(content, 0, 0);

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
        actions.Controls.Add(_btnAdd);
        root.Controls.Add(actions, 0, 2);

        Controls.Add(root);

        _btnAdd.Click += (_, _) =>
        {
            if (_selectedVariantId == Guid.Empty)
            {
                MessageBox.Show(AppStrings.PleaseSelectVariant, AppStrings.Warning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            DialogResult = DialogResult.OK;
            Close();
        };
        _btnClose.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
    }

    private async Task LoadAsync()
    {
        _variants = await _context.ProductVariants
            .AsNoTracking()
            .Where(v => v.ProductId == _productId && v.IsActive)
            .OrderByDescending(v => v.IsDefault)
            .ToListAsync();

        // Load attributes for each variant
        var variantIds = _variants.Select(v => v.Id).ToList();
        var variantAttrs = await _context.VariantAttributes
            .AsNoTracking()
            .Where(a => variantIds.Contains(a.VariantId))
            .Include(a => a.Definition)
            .Include(a => a.Value)
            .ToListAsync();

        foreach (var va in variantAttrs)
        {
            if (!_variantAttrs.TryGetValue(va.VariantId, out var list))
            {
                list = new List<AttributeValue>();
                _variantAttrs[va.VariantId] = list;
            }
            list.Add(va.Value);
        }

        // Load attribute definitions for this product
        var defs = await _context.AttributeDefinitions
            .AsNoTracking()
            .Include(d => d.Values)
            .Where(d => d.ProductId == _productId)
            .OrderBy(d => d.SortOrder)
            .ToListAsync();

        // Build attribute selection groups
        foreach (var def in defs)
        {
            var groupLabel = new Label
            {
                Text = def.Name,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = AppColors.Ink,
                AutoSize = true,
                Margin = new Padding(0, 8, 0, 4)
            };
            _attrPanel.Controls.Add(groupLabel);

            var chipPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = true,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Margin = new Padding(0, 0, 0, 8)
            };

            foreach (var val in def.Values.OrderBy(v => v.CreatedAt))
            {
                var chip = new Button
                {
                    Text = val.Value,
                    AutoSize = true,
                    MinimumSize = new Size(60, 28),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = AppColors.Surface,
                    ForeColor = AppColors.Ink,
                    Margin = new Padding(4, 2, 4, 2),
                    Tag = (def.Id, val.Id)
                };
                chip.FlatAppearance.BorderColor = AppColors.Border;
                chip.Click += (_, _) => OnChipClicked(def.Id, val.Id, chip, chipPanel);
                chipPanel.Controls.Add(chip);
            }

            _attrPanel.Controls.Add(chipPanel);
        }

        // Select first variant by default
        if (_variants.Count > 0)
        {
            SelectVariant(_variants[0].Id);
        }
    }

    private void OnChipClicked(Guid defId, Guid valueId, Button chip, FlowLayoutPanel panel)
    {
        // Deselect other chips in this group
        foreach (Control c in panel.Controls)
        {
            if (c is Button b)
            {
                b.BackColor = AppColors.Surface;
                b.ForeColor = AppColors.Ink;
            }
        }
        chip.BackColor = AppColors.Primary;
        chip.ForeColor = AppColors.OnDark;

        _attrSelection[defId] = valueId;
        ResolveVariant();
    }

    private void ResolveVariant()
    {
        if (_attrSelection.Count == 0)
        {
            if (_variants.Count > 0) SelectVariant(_variants[0].Id);
            return;
        }

        // Find variant matching all selected attribute values
        foreach (var v in _variants)
        {
            if (!_variantAttrs.TryGetValue(v.Id, out var attrs)) continue;
            var valueIds = attrs.Select(a => a.Id).ToHashSet();
            bool match = _attrSelection.Values.All(sv => valueIds.Contains(sv));
            if (match)
            {
                SelectVariant(v.Id);
                return;
            }
        }
    }

    private void SelectVariant(Guid variantId)
    {
        var variant = _variants.FirstOrDefault(v => v.Id == variantId);
        if (variant is null) return;

        _selectedVariantId = variantId;
        _lblVariantName.Text = variant.Name;
        _lblPrice.Text = variant.SalePrice.Amount.ToString("N2");
        _lblBarcode.Text = variant.SKU ?? "";

        // Load image
        if (!string.IsNullOrWhiteSpace(variant.ThumbnailPath) || !string.IsNullOrWhiteSpace(variant.ImagePath))
        {
            var path = variant.ThumbnailPath ?? variant.ImagePath;
            try
            {
                var full = Path.Combine(DzentoPOS2.Core.Configuration.AppPaths.Root, "images", (path ?? "").Replace('/', '\\'));
                if (System.IO.File.Exists(full))
                {
                    using var fs = new System.IO.FileStream(full, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                    using var img = Image.FromStream(fs);
                    _picPreview.Image = new Bitmap(img);
                }
            }
            catch { /* best effort */ }
        }
    }

    protected override void ApplyTheme()
    {
        base.ApplyTheme();
        _btnAdd.BackColor = AppColors.Primary;
        _btnAdd.ForeColor = AppColors.OnDark;
    }
}
