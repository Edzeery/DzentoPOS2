using DzentoPOS2.Core.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DzentoPOS2.Data.Configurations.Sales
{
    public class SalesQuotationConfiguration
        : IEntityTypeConfiguration<SalesQuotation>
    {
        public void Configure(EntityTypeBuilder<SalesQuotation> builder)
        {
            builder.ToTable("SalesQuotations");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.QuotationNumber)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasIndex(x => x.QuotationNumber)
                .IsUnique();

            builder.Property(x => x.Total)
                .HasPrecision(18, 2);

            builder.Property(x => x.Discount)
                .HasPrecision(18, 2);

            builder.Property(x => x.ExpiryDate)
                .IsRequired();

            builder.Property(x => x.Notes)
                .HasMaxLength(500);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.IsDeleted)
                .HasDefaultValue(false);
        }
    }
}