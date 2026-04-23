using TaskFlow.Platform.Domain.Users.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TaskFlow.Platform.Persistence.Users.EntityConfigurations;

public sealed class AddressEntityConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.ToTable("Addresses");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Street).IsRequired();
        builder.Property(x => x.City).IsRequired();
        builder.Property(x => x.PostalCode).IsRequired();
        builder.Property(x => x.Country).IsRequired();
    }
}
