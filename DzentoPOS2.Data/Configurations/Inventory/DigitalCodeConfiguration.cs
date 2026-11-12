using DzentoPOS2.Core.Entities.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DzentoPOS2.Data.Configurations.Inventory
{
    public class DigitalCodeConfiguration : IEntityTypeConfiguration<DigitalCode>
    {
        public void Configure(EntityTypeBuilder<DigitalCode> builder)
        {
            builder.ToTable("DigitalCodes");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Code)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Pin)
                .HasMaxLength(200);

            builder.Property(x => x.Status);

            // A code is unique per variant; the same code may be reused by a
            // different product/variant (e.g. the same recharge denomination).
            builder.HasIndex(x => new { x.VariantId, x.Code })
                .IsUnique();

            builder.HasIndex(x => x.Status);
            builder.HasIndex(x => x.SalesInvoiceId);

            builder.HasOne(x => x.Variant)
                .WithMany()
                .HasForeignKey(x => x.VariantId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
