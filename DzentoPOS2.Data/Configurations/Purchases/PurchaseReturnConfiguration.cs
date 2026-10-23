using DzentoPOS2.Core.Entities.Purchases;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DzentoPOS2.Data.Configurations.Purchases
{
    public class PurchaseReturnConfiguration
        : IEntityTypeConfiguration<PurchaseReturn>
    {
        public void Configure(EntityTypeBuilder<PurchaseReturn> builder)
        {
            builder.ToTable("PurchaseReturns");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ReturnNumber)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasIndex(x => x.ReturnNumber)
                .IsUnique();

            builder.Property(x => x.TotalAmount)
                .HasPrecision(18, 2);

            builder.Property(x => x.Notes)
                .HasMaxLength(500);

            builder.Property(x => x.ReturnDate)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.IsDeleted)
                .HasDefaultValue(false);

            builder.HasMany(x => x.Items)
                .WithOne(x => x.PurchaseReturn)
                .HasForeignKey(x => x.PurchaseReturnId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}