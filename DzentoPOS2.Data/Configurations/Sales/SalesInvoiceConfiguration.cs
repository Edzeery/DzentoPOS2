using DzentoPOS2.Core.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DzentoPOS2.Data.Configurations.Sales
{
    public class SalesInvoiceConfiguration
        : IEntityTypeConfiguration<SalesInvoice>
    {
        public void Configure(EntityTypeBuilder<SalesInvoice> builder)
        {
            builder.ToTable("SalesInvoices");

            builder.HasKey(x => x.Id);

            builder.OwnsOne(x => x.InvoiceNumber, n =>
            {
                n.Property(p => p.Value)
                 .HasColumnName("InvoiceNumber")
                 .HasMaxLength(100)
                 .IsRequired();

                n.HasIndex(p => p.Value).IsUnique();
            });

            builder.OwnsOne(x => x.Subtotal, m =>
            {
                m.Property(p => p.Amount)
                    .HasColumnName("SubtotalAmount")
                    .HasPrecision(18, 2)
                    .IsRequired();

                m.Property(p => p.Currency)
                    .HasColumnName("SubtotalCurrency")
                    .HasMaxLength(10);
            });

            builder.OwnsOne(x => x.DiscountAmount, m =>
            {
                m.Property(p => p.Amount)
                    .HasColumnName("DiscountAmount")
                    .HasPrecision(18, 2)
                    .IsRequired();

                m.Property(p => p.Currency)
                    .HasColumnName("DiscountCurrency")
                    .HasMaxLength(10);
            });

            builder.OwnsOne(x => x.TaxAmount, m =>
            {
                m.Property(p => p.Amount)
                    .HasColumnName("TaxAmount")
                    .HasPrecision(18, 2)
                    .IsRequired();

                m.Property(p => p.Currency)
                    .HasColumnName("TaxCurrency")
                    .HasMaxLength(10);
            });

            builder.OwnsOne(x => x.Total, m =>
            {
                m.Property(p => p.Amount)
                    .HasColumnName("TotalAmount")
                    .HasPrecision(18, 2)
                    .IsRequired();

                m.Property(p => p.Currency)
                    .HasColumnName("TotalCurrency")
                    .HasMaxLength(10);
            });

            builder.OwnsOne(x => x.Discount, d =>
            {
                d.Property(p => p.Value)
                 .HasColumnName("Discount")
                 .HasPrecision(18, 2);
            });

            builder.OwnsOne(x => x.Tax, t =>
            {
                t.Property(p => p.Rate)
                 .HasColumnName("TaxRate")
                 .HasPrecision(5, 2);
            });

            builder.OwnsOne(x => x.PaidAmount, m =>
            {
                m.Property(p => p.Amount).HasColumnName("PaidAmount").HasPrecision(18, 2);
                m.Property(p => p.Currency).HasColumnName("PaidCurrency").HasMaxLength(10);
            });

            builder.OwnsOne(x => x.RemainingAmount, m =>
            {
                m.Property(p => p.Amount).HasColumnName("RemainingAmount").HasPrecision(18, 2);
                m.Property(p => p.Currency).HasColumnName("RemainingCurrency").HasMaxLength(10);
            });

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.IsDeleted)
                .HasDefaultValue(false);

            builder.Property(x => x.VoidReason)
                .HasMaxLength(500);

            builder.HasIndex(x => x.IsVoided);

            builder.HasIndex(x => x.CashSessionId);

            builder.HasOne<DzentoPOS2.Core.Entities.Accounting.CashSession>()
                .WithMany()
                .HasForeignKey(x => x.CashSessionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Items)
                .WithOne()
                .HasForeignKey(x => x.SalesInvoiceId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}