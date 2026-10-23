using DzentoPOS2.Core.Entities.Purchases;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DzentoPOS2.Data.Configurations.Purchases
{
    public class PurchaseReturnItemConfiguration
        : IEntityTypeConfiguration<PurchaseReturnItem>
    {
        public void Configure(EntityTypeBuilder<PurchaseReturnItem> builder)
        {
            builder.ToTable("PurchaseReturnItems");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Quantity)
                .HasPrecision(18, 2);

            builder.Property(x => x.CostPrice)
                .HasPrecision(18, 2);

            builder.Property(x => x.Total)
                .HasPrecision(18, 2);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.IsDeleted)
                .HasDefaultValue(false);

            builder.HasOne(x => x.PurchaseReturn)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.PurchaseReturnId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}