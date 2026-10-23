using DzentoPOS2.Core.Entities.Partners;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DzentoPOS2.Data.Configurations.Partners
{
    public class CustomerConfiguration
        : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.ToTable("Customers");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.FullName)
                .IsRequired()
                .HasMaxLength(200);

            builder.OwnsOne(x => x.PhoneNumber, phone =>
            {
                phone.Property(x => x.Value)
                    .HasColumnName("PhoneNumber")
                    .HasMaxLength(30);

                phone.WithOwner();
            });


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

            builder.Property(x => x.Balance)
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.IsActive)
                .HasDefaultValue(true);

            builder.HasIndex(x => x.FullName);
             
        }
    }
}