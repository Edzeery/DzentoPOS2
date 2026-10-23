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
    public class CurrencyConfiguration
        : IEntityTypeConfiguration<Currency>
    {
        public void Configure(EntityTypeBuilder<Currency> builder)
        {
            builder.ToTable("Currencies");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Code)
                .IsRequired()
                .HasMaxLength(10);

            builder.Property(x => x.Symbol)
                .IsRequired()
                .HasMaxLength(10);

            builder.Property(x => x.ExchangeRate)
                .HasColumnType("decimal(18,6)");

            builder.Property(x => x.IsDefault)
                .HasDefaultValue(false);

            builder.HasIndex(x => x.Name);

            builder.HasIndex(x => x.Code)
                .IsUnique();
        }
    }
}