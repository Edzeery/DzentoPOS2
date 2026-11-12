using DzentoPOS2.Core.Entities.Inventory;
using DzentoPOS2.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DzentoPOS2.Data.Configurations.Inventory
{
    public class SerialUnitConfiguration : IEntityTypeConfiguration<SerialUnit>
    {
        public void Configure(EntityTypeBuilder<SerialUnit> builder)
        {
            builder.ToTable("SerialUnits");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.SerialNumber).HasMaxLength(100);
            builder.Property(x => x.Imei1).HasMaxLength(20);
            builder.Property(x => x.Imei2).HasMaxLength(20);
            builder.Property(x => x.UnitCost).HasPrecision(18, 2);

            // Unique index on Imei1 (where not null)
            builder.HasIndex(x => x.Imei1)
                .IsUnique()
                .HasFilter("[Imei1] IS NOT NULL");

            // Unique index on (VariantId, SerialNumber) (where SerialNumber not null)
            builder.HasIndex(x => new { x.VariantId, x.SerialNumber })
                .IsUnique()
                .HasFilter("[SerialNumber] IS NOT NULL");

            builder.HasIndex(x => x.Status);
            builder.HasIndex(x => x.WarehouseId);
            builder.HasIndex(x => x.BatchId);

            builder.HasOne(x => x.Batch)
                .WithMany()
                .HasForeignKey(x => x.BatchId)
                .OnDelete(DeleteBehavior.SetNull);

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
