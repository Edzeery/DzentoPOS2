using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DzentoPOS2.Core.Entities.Businesses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DzentoPOS2.Data.Configurations.Businesses
{
    public class BranchConfiguration : IEntityTypeConfiguration<Branch>
    {
        public void Configure(EntityTypeBuilder<Branch> builder)
        {
            builder.ToTable("Branches");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(150);

            // ❌ لا يوجد Property هنا لأن Address ValueObject

            builder.OwnsOne(x => x.Address, address =>
            {
                address.Property(x => x.Country)
                    .HasMaxLength(100)
                    .HasColumnName("Country");

                address.Property(x => x.City)
                    .HasMaxLength(100)
                    .HasColumnName("City");

                address.Property(x => x.Street)
                    .HasMaxLength(200)
                    .HasColumnName("Street");
            });

            builder.OwnsOne(x => x.PhoneNumber, phone =>
            {
                phone.Property(x => x.Value)
                    .HasColumnName("PhoneNumber")
                    .HasMaxLength(30);

                phone.WithOwner();
            });


            builder.Property(x => x.IsMainBranch)
                .HasDefaultValue(false);

            builder.Property(x => x.IsActive)
                .HasDefaultValue(true);

            builder.HasIndex(x => x.Name);

            builder.HasMany(x => x.Warehouses)
                .WithOne(x => x.Branch)
                .HasForeignKey(x => x.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.CashRegisters)
                .WithOne(x => x.Branch)
                .HasForeignKey(x => x.BranchId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}