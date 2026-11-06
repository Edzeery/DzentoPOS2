using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DzentoPOS2.App.Services.Sales;
using DzentoPOS2.Core.Configuration;
using DzentoPOS2.Core.Entities.Inventory;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Localization.Resources;
using DzentoPOS2.WinForms.Helpers;
using DzentoPOS2.WinForms.Themes;
using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.WinForms.Forms.Sales;

/// <summary>
/// Shared barcode picker. Shown when a barcode maps to multiple variants.
/// Returns the selected variant via <see cref="SelectedVariantId"/>.
/// </summary>
public class SharedBarcodePickerDialog : ThemedDialog
{
    private readonly AppDbContext _context;
    private readonly IReadOnlyList<Guid> _variantIds;
    private readonly FlowLayoutPanel _list = new() { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, WrapContents = true, AutoScroll = true };

    private Guid _selectedVariantId;
    public Guid SelectedVariantId => _selectedVariantId;
    public string SelectedVariantName { get; private set; } = string.Empty;
    public decimal SelectedPrice { get; private set; }

    public SharedBarcodePickerDialog(AppDbContext context, IReadOnlyList<Guid> variantIds)
    {
        _context = context;
        _variantIds = variantIds;

        Text = AppStrings.SelectSharedBarcode;
        Size = new Size(480, 400);
        MinimumSize = new Size(320, 280);
        StartPosition = FormStartPosition.CenterParent;
        AutoScaleMode = AutoScaleMode.Font;

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(12),
            Margin = Padding.Empty
        };
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var lblHint = new Label
        {
            Text = AppStrings.SharedBarcodeHint,
            Dock = DockStyle.Top,
            AutoSize = true,
            ForeColor = AppColors.Ink,
            Margin = new Padding(0, 0, 0, 8)
        };
        root.Controls.Add(lblHint, 0, 0);

        var scrollPanel = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
        scrollPanel.Controls.Add(_list);
        root.Controls.Add(scrollPanel, 0, 1);

        Controls.Add(root);

        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        var variants = await _context.ProductVariants
            .AsNoTracking()
            .Where(v => _variantIds.Contains(v.Id) && v.IsActive)
            .Include(v => v.Product)
            .ToListAsync();

        foreach (var v in variants)
        {
            var card = BuildCard(v);
            _list.Controls.Add(card);
        }
    }

    private Control BuildCard(ProductVariant variant)
    {
        var card = new TableLayoutPanel
        {
            Width = 440,
            Height = 80,
            ColumnCount = 3,
            RowCount = 1,
            Margin = new Padding(4),
            Padding = new Padding(8),
            BackColor = AppColors.Surface
        };
        card.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        card.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        card.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        card.RowStyles[0] = new RowStyle(SizeType.Percent, 100F);
        card.RowStyles[1] = new RowStyle(SizeType.Percent, 100F);
        card.RowStyles[2] = new RowStyle(SizeType.Percent, 100F);

        var pic = new PictureBox
        {
            SizeMode = PictureBoxSizeMode.Zoom,
            Size = new Size(64, 64),
            Margin = new Padding(0, 0, 8, 0)
        };
        if (!string.IsNullOrWhiteSpace(variant.ThumbnailPath) || !string.IsNullOrWhiteSpace(variant.ImagePath))
        {
            try
            {
                var path = variant.ThumbnailPath ?? variant.ImagePath;
                var full = Path.Combine(AppPaths.Root, "images", (path ?? "").Replace('/', '\\'));
                if (System.IO.File.Exists(full))
                {
                    using var fs = new System.IO.FileStream(full, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                    using var img = Image.FromStream(fs);
                    pic.Image = new Bitmap(img);
                }
            }
            catch (Exception) { /* best effort image load */ }
        }
        card.Controls.Add(pic, 0, 0);

        var info = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Margin = Padding.Empty
        };
        info.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        info.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        info.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var lblName = new Label
        {
            Text = variant.Name,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = AppColors.Ink,
            AutoSize = true
        };
        var lblPrice = new Label
        {
            Text = variant.SalePrice.Amount.ToString("N2"),
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = AppColors.Primary,
            AutoSize = true
        };
        var lblStock = new Label
        {
            Text = $"{AppStrings.StockLabel}: {variant.Product?.Name ?? ""}",
            Font = new Font("Segoe UI", 8.5F),
            ForeColor = AppColors.InkMuted,
            AutoSize = true
        };
        info.Controls.Add(lblName, 0, 0);
        info.Controls.Add(lblPrice, 0, 1);
        info.Controls.Add(lblStock, 0, 2);
        card.Controls.Add(info, 1, 0);

        var btnSelect = new Button
        {
            Text = AppStrings.Select,
            AutoSize = true,
            MinimumSize = new Size(80, 32),
            FlatStyle = FlatStyle.Flat,
            BackColor = AppColors.Primary,
            ForeColor = AppColors.OnDark
        };
        btnSelect.Click += (_, _) =>
        {
            _selectedVariantId = variant.Id;
            SelectedVariantName = variant.Name;
            SelectedPrice = variant.SalePrice.Amount;
            DialogResult = DialogResult.OK;
            Close();
        };
        card.Controls.Add(btnSelect, 2, 0);

        return card;
    }

    protected override void ApplyTheme()
    {
        base.ApplyTheme();
    }
}
