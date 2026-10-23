using DzentoPOS2.Core.Entities.Systems;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DzentoPOS2.Data.Configurations.Systems
{
    public class AuditLogConfiguration
        : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            builder.ToTable("AuditLogs");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Action)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.EntityName)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(x => x.OldValues);

            builder.Property(x => x.NewValues);

            builder.Property(x => x.IpAddress)
                .HasMaxLength(100);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.IsDeleted)
                .HasDefaultValue(false);

            builder.HasIndex(x => x.UserId);

            builder.HasIndex(x => x.EntityName);
        }
    }
}