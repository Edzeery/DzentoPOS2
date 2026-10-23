using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DzentoPOS2.Core.Entities.Licensing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DzentoPOS2.Data.Configurations.Licensing
{
    public class LicenseConfiguration
        : IEntityTypeConfiguration<License>
    {
        public void Configure(EntityTypeBuilder<License> builder)
        {
            builder.ToTable("Licenses");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.LicenseKey)
                .IsRequired()
                .HasMaxLength(300);

            builder.Property(x => x.StartDate)
                .IsRequired();

            builder.Property(x => x.ExpiryDate)
                .IsRequired();

            builder.Property(x => x.IsActive)
                .HasDefaultValue(true);

            builder.HasIndex(x => x.LicenseKey)
                .IsUnique();

            builder.HasIndex(x => x.LicensePlanId);

            builder.HasOne(x => x.LicensePlan)
                .WithMany()
                .HasForeignKey(x => x.LicensePlanId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}