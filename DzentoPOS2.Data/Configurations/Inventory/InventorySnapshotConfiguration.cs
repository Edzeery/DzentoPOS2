using DzentoPOS2.Core.Entities.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DzentoPOS2.Data.Configurations.Inventory
{
    public class InventorySnapshotConfiguration : IEntityTypeConfiguration<InventorySnapshot>
    {
        public void Configure(EntityTypeBuilder<InventorySnapshot> builder)
        {
            builder.ToTable("InventorySnapshots");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.SnapshotDate).IsRequired();
            // Legacy aggregate (BranchId IS NULL): unique on SnapshotDate
            builder.HasIndex(x => x.SnapshotDate)
                .HasFilter("[BranchId] IS NULL")
                .IsUnique();
            // Per-branch: unique on (BranchId, SnapshotDate)
            builder.HasIndex(x => new { x.BranchId, x.SnapshotDate })
                .HasFilter("[BranchId] IS NOT NULL")
                .IsUnique();

            builder.Property(x => x.TotalQuantity).HasPrecision(18, 2);
            builder.Property(x => x.CostValue).HasPrecision(18, 2);
            builder.Property(x => x.RetailValue).HasPrecision(18, 2);
            builder.Property(x => x.ExpectedProfit).HasPrecision(18, 2);
            builder.Property(x => x.AtRiskProfit).HasPrecision(18, 2);
            builder.Property(x => x.DiscountFactorAvg).HasPrecision(18, 4);
            builder.Property(x => x.FeeFactor).HasPrecision(18, 4);
        }
    }
}