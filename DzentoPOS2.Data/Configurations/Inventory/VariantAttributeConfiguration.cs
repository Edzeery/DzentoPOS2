using DzentoPOS2.Core.Entities.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DzentoPOS2.Data.Configurations.Inventory
{
    public class VariantAttributeConfiguration : IEntityTypeConfiguration<VariantAttribute>
    {
        public void Configure(EntityTypeBuilder<VariantAttribute> builder)
        {
            builder.ToTable("VariantAttributes");
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => new { x.VariantId, x.DefinitionId }).IsUnique();
            builder.HasOne(x => x.Variant).WithMany().HasForeignKey(x => x.VariantId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(x => x.Definition).WithMany().HasForeignKey(x => x.DefinitionId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Value).WithMany().HasForeignKey(x => x.ValueId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}