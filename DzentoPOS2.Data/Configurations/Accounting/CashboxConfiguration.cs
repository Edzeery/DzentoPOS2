using DzentoPOS2.Core.Entities.Accounting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DzentoPOS2.Data.Configurations.Accounting
{
    public class CashboxConfiguration
        : IEntityTypeConfiguration<Cashbox>
    {
        public void Configure(EntityTypeBuilder<Cashbox> builder)
        {
            builder.ToTable("Cashboxes");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(x => x.CurrentBalance)
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.IsActive)
                .HasDefaultValue(true);

            builder.HasIndex(x => x.Name);

            builder.HasIndex(x => x.BranchId);
        }
    }
}