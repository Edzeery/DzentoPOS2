using System;
using DzentoPOS2.Core.Entities.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DzentoPOS2.Data.Configurations.Inventory
{
    public class ProductBarcodeConfiguration
        : IEntityTypeConfiguration<ProductBarcode>
    {
        public void Configure(EntityTypeBuilder<ProductBarcode> builder)
        {
            builder.ToTable("ProductBarcodes");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Barcode)
                .IsRequired()
                .HasMaxLength(64);

            builder.HasIndex(x => x.Barcode)
                .IsUnique();

            builder.HasIndex(x => x.VariantId);

            builder.Property(x => x.ProductId)
                .IsRequired(false);

            builder.HasOne(x => x.Product)
                .WithMany(x => x.Barcodes)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(x => x.Variant)
                .WithMany()
                .HasForeignKey(x => x.VariantId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}