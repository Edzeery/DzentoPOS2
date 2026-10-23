using DzentoPOS2.Core.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DzentoPOS2.Data.Configurations.Sales
{
    public class SalesReturnConfiguration
        : IEntityTypeConfiguration<SalesReturn>
    {
        public void Configure(EntityTypeBuilder<SalesReturn> builder)
        {
            builder.ToTable("SalesReturns");

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
                .WithOne(x => x.SalesReturn)
                .HasForeignKey(x => x.SalesReturnId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}