using DzentoPOS2.Core.Entities.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DzentoPOS2.Data.Configurations.Inventory
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("Products");

            builder.HasKey(x => x.Id);
         

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.SKU)
                .HasMaxLength(100);

            builder.Property(x => x.Barcode)
                .HasMaxLength(64);

            builder.Property(x => x.Description)
                .HasMaxLength(1000);

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

            builder.HasIndex(x => x.Name);

            builder.HasIndex(x => x.SKU)
                .IsUnique();

            builder.HasIndex(x => x.Barcode)
                .IsUnique();

            builder.HasOne(x => x.Category)
                .WithMany(x => x.Products)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(x => new { x.Name, x.CategoryId });
            builder.HasOne(x => x.Unit)
                .WithMany(x => x.Products)
                .HasForeignKey(x => x.UnitId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}