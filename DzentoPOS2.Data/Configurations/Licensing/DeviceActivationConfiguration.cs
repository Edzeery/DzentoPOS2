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
    public class DeviceActivationConfiguration
        : IEntityTypeConfiguration<DeviceActivation>
    {
        public void Configure(EntityTypeBuilder<DeviceActivation> builder)
        {
            builder.ToTable("DeviceActivations");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.DeviceName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.DeviceIdentifier)
                .IsRequired()
                .HasMaxLength(300);

            builder.Property(x => x.ActivatedAt)
                .IsRequired();

            builder.HasIndex(x => x.DeviceIdentifier);

            builder.HasIndex(x => x.LicenseId);

            builder.HasOne(x => x.License)
                .WithMany(x => x.DeviceActivations)
                .HasForeignKey(x => x.LicenseId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}