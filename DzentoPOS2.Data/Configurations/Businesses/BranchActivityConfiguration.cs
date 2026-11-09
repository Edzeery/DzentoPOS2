using DzentoPOS2.Core.Entities.Businesses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DzentoPOS2.Data.Configurations.Businesses
{
    public class BranchActivityConfiguration : IEntityTypeConfiguration<BranchActivity>
    {
        public void Configure(EntityTypeBuilder<BranchActivity> builder)
        {
            builder.ToTable("BranchActivities");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ActivityType)
                .IsRequired();

            builder.Property(x => x.IsEnabled)
                .HasDefaultValue(true);

            builder.HasIndex(x => new { x.BranchId, x.ActivityType })
                .IsUnique();

            builder.HasOne(x => x.Branch)
                .WithMany()
                .HasForeignKey(x => x.BranchId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
