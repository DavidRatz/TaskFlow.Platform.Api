using TaskFlow.Platform.Domain.Users.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TaskFlow.Platform.Persistence.Users.EntityConfigurations;

public sealed class UserEntityConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FirstName).IsRequired();
        builder.Property(x => x.LastName).IsRequired();

        builder.HasOne(x => x.IdentityUser)
            .WithOne(x => x.User)
            .HasForeignKey<User>(x => x.Id)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
