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
    public class StockTransferItemConfiguration
        : IEntityTypeConfiguration<StockTransferItem>
    {
        public void Configure(EntityTypeBuilder<StockTransferItem> builder)
        {
            builder.ToTable("StockTransferItems");

            builder.HasKey(x => x.Id);

            builder.OwnsOne(x => x.Quantity, q =>
            {
                q.Property(p => p.Value)
                    .HasColumnName("Quantity")
                    .HasPrecision(18, 2);
            });

            builder.HasIndex(x => x.StockTransferId);

            builder.HasIndex(x => x.ProductId);

            builder.HasOne(x => x.StockTransfer)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.StockTransferId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}