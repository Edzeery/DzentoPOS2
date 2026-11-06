using DzentoPOS2.Core.Entities.Systems;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DzentoPOS2.Data.Configurations.Systems;

public class DocumentSequenceConfiguration
    : IEntityTypeConfiguration<DocumentSequence>
{
    public void Configure(EntityTypeBuilder<DocumentSequence> builder)
    {
        builder.ToTable("DocumentSequences");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Prefix)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.DateKey)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.LastValue)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.IsDeleted)
            .HasDefaultValue(false);

        builder.HasIndex(x => new { x.Prefix, x.DateKey })
            .IsUnique();
    }
}
