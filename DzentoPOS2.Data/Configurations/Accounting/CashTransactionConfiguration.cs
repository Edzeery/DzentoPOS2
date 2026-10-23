using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DzentoPOS2.Core.Entities.Accounting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DzentoPOS2.Data.Configurations.Accounting
{
    public class CashTransactionConfiguration
        : IEntityTypeConfiguration<CashTransaction>
    {
        public void Configure(EntityTypeBuilder<CashTransaction> builder)
        {
            builder.ToTable("CashTransactions");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Amount)
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.TransactionType)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.ReferenceNumber)
                .HasMaxLength(100);

            builder.Property(x => x.Notes)
                .HasMaxLength(1000);

            builder.Property(x => x.TransactionDate)
                .IsRequired();

            builder.Property(x => x.SourceType)
                .HasMaxLength(50);

            builder.HasIndex(x => x.CashboxId);

            builder.HasIndex(x => x.CashSessionId);

            builder.HasIndex(x => x.TransactionDate);

            builder.HasIndex(x => x.ReferenceNumber);

            builder.HasIndex(x => new { x.SourceType, x.SourceId });

            builder.HasOne<DzentoPOS2.Core.Entities.Accounting.CashSession>()
                .WithMany()
                .HasForeignKey(x => x.CashSessionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}