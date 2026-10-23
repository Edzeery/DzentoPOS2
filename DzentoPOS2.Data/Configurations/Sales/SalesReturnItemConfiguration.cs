using DzentoPOS2.Core.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DzentoPOS2.Data.Configurations.Sales
{
    public class SalesReturnItemConfiguration
        : IEntityTypeConfiguration<SalesReturnItem>
    {
        public void Configure(EntityTypeBuilder<SalesReturnItem> builder)
        {
            builder.ToTable("SalesReturnItems");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Quantity)
                .HasPrecision(18, 2);

            builder.Property(x => x.Price)
                .HasPrecision(18, 2);

            builder.Property(x => x.Total)
                .HasPrecision(18, 2);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.IsDeleted)
                .HasDefaultValue(false);

            builder.HasOne(x => x.SalesReturn)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.SalesReturnId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}