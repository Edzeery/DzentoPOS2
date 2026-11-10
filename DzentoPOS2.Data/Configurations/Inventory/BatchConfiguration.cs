using DzentoPOS2.Core.Entities.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DzentoPOS2.Data.Configurations.Inventory
{
    public class BatchConfiguration : IEntityTypeConfiguration<Batch>
    {
        public void Configure(EntityTypeBuilder<Batch> builder)
        {
            builder.ToTable("Batches");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.BatchNumber)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Quantity)
                .HasPrecision(18, 3);

            builder.Property(x => x.UnitCost)
                .HasPrecision(18, 2);

            builder.HasIndex(x => x.VariantId);
            builder.HasIndex(x => x.WarehouseId);
            builder.HasIndex(x => x.ExpiryDate);

            builder.HasOne(x => x.Variant)
                .WithMany()
                .HasForeignKey(x => x.VariantId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Warehouse)
                .WithMany()
                .HasForeignKey(x => x.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
