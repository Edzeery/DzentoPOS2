using DzentoPOS2.Core.Entities.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DzentoPOS2.Data.Configurations.Inventory
{
    public class AttributeValueConfiguration : IEntityTypeConfiguration<AttributeValue>
    {
        public void Configure(EntityTypeBuilder<AttributeValue> builder)
        {
            builder.ToTable("AttributeValues");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Value).IsRequired().HasMaxLength(200);
            builder.HasIndex(x => new { x.DefinitionId, x.Value }).IsUnique();
            builder.HasOne(x => x.Definition).WithMany(x => x.Values).HasForeignKey(x => x.DefinitionId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}