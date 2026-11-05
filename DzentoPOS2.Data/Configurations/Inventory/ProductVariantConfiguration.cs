using DzentoPOS2.Core.Entities.Inventory;
using DzentoPOS2.Core.ValueObjects.Common;
using DzentoPOS2.Core.ValueObjects.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DzentoPOS2.Data.Configurations.Inventory
{
    public class ProductVariantConfiguration
        : IEntityTypeConfiguration<ProductVariant>
    {
        public void Configure(EntityTypeBuilder<ProductVariant> builder)
        {
            builder.ToTable("ProductVariants");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(x => x.SKU)
                .HasMaxLength(100);

            builder.HasIndex(x => x.SKU)
                .IsUnique();

            builder.Property(x => x.AdditionalPrice)
                .HasColumnType("decimal(18,2)");

            builder.OwnsOne(x => x.CostPrice, m =>
            {
                m.Property(p => p.Amount)
                    .HasColumnName("CostPriceAmount")
                    .HasPrecision(18, 2);
                m.Property(p => p.Currency)
                    .HasColumnName("CostPriceCurrency")
                    .HasMaxLength(10);
            });

            builder.OwnsOne(x => x.SalePrice, m =>
            {
                m.Property(p => p.Amount)
                    .HasColumnName("SalePriceAmount")
                    .HasPrecision(18, 2);
                m.Property(p => p.Currency)
                    .HasColumnName("SalePriceCurrency")
                    .HasMaxLength(10);
            });

            builder.OwnsOne(x => x.MinimumStock, q =>
            {
                q.Property(p => p.Value)
                    .HasColumnName("MinimumStock")
                    .HasPrecision(18, 2);
            });

            builder.Property(x => x.ImagePath)
                .HasMaxLength(500);

            builder.Property(x => x.ThumbnailPath)
                .HasMaxLength(500);

            builder.HasIndex(x => x.ProductId);

            builder.HasOne(x => x.Product)
                .WithMany(x => x.Variants)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}