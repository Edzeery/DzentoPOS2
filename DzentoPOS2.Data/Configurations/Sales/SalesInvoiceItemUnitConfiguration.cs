using DzentoPOS2.Core.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DzentoPOS2.Data.Configurations.Sales
{
    public class SalesInvoiceItemUnitConfiguration : IEntityTypeConfiguration<SalesInvoiceItemUnit>
    {
        public void Configure(EntityTypeBuilder<SalesInvoiceItemUnit> builder)
        {
            builder.ToTable("SalesInvoiceItemUnits");

            builder.HasKey(x => x.Id);

            builder.HasIndex(x => new { x.SalesInvoiceItemId, x.SerialUnitId })
                .IsUnique();

            builder.HasOne(x => x.SalesInvoiceItem)
                .WithMany()
                .HasForeignKey(x => x.SalesInvoiceItemId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.SerialUnit)
                .WithMany()
                .HasForeignKey(x => x.SerialUnitId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
