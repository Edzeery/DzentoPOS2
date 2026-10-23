using DzentoPOS2.Core.Entities.Accounting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DzentoPOS2.Data.Configurations.Accounting
{
    public class CashSessionConfiguration
        : IEntityTypeConfiguration<CashSession>
    {
        public void Configure(EntityTypeBuilder<CashSession> builder)
        {
            builder.ToTable("CashSessions");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.OpeningBalance)
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.ClosingBalance)
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.ExpectedBalance)
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.Difference)
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.Notes)
                .HasMaxLength(1000);

            builder.HasIndex(x => x.CashboxId);

            builder.HasIndex(x => x.OpenedAtUtc);

            builder.HasIndex(x => x.Status);

            builder.HasOne<Cashbox>()
                .WithMany()
                .HasForeignKey(x => x.CashboxId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
