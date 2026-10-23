using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                .HasMaxLength(200);

            builder.HasIndex(x => x.Barcode)
                .IsUnique();

            builder.HasOne(x => x.Product)
                .WithMany(x => x.Barcodes)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}