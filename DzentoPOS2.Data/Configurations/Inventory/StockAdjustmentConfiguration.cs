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
    public class StockAdjustmentConfiguration
        : IEntityTypeConfiguration<StockAdjustment>
    {
        public void Configure(EntityTypeBuilder<StockAdjustment> builder)
        {
            builder.ToTable("StockAdjustments");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ReferenceNumber)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Reason)
                .HasMaxLength(1000);

            builder.Property(x => x.AdjustmentDate)
                .IsRequired();

            builder.HasIndex(x => x.ReferenceNumber)
                .IsUnique();

            builder.HasIndex(x => x.AdjustmentDate);

            builder.HasOne(x => x.Warehouse)
                .WithMany()
                .HasForeignKey(x => x.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.CreatedByUser)
                .WithMany()
                .HasForeignKey(x => x.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Transactions)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}