using DzentoPOS2.Core.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DzentoPOS2.Data.Configurations.Sales
{
    public class SalesPaymentConfiguration
        : IEntityTypeConfiguration<SalesPayment>
    {
        public void Configure(EntityTypeBuilder<SalesPayment> builder)
        {
            builder.ToTable("SalesPayments");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Amount)
                .HasPrecision(18, 2);

            builder.Property(x => x.PaymentMethod)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.ReferenceNumber)
                .HasMaxLength(150);

            builder.Property(x => x.Notes)
                .HasMaxLength(500);

            builder.Property(x => x.PaymentDate)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.IsDeleted)
                .HasDefaultValue(false);

            builder.HasOne(x => x.SalesInvoice)
                .WithMany()
                .HasForeignKey(x => x.SalesInvoiceId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}