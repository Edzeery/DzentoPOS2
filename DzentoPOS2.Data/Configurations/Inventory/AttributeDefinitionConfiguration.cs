using DzentoPOS2.Core.Entities.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DzentoPOS2.Data.Configurations.Inventory
{
    public class AttributeDefinitionConfiguration : IEntityTypeConfiguration<AttributeDefinition>
    {
        public void Configure(EntityTypeBuilder<AttributeDefinition> builder)
        {
            builder.ToTable("AttributeDefinitions");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
            builder.HasIndex(x => new { x.ProductId, x.Name }).IsUnique();
            builder.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}