using TaskFlow.Platform.Domain.Auth.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TaskFlow.Platform.Persistence.Authentications.EntityConfigurations;

public sealed class RevokedTokenConfiguration : IEntityTypeConfiguration<RevokedToken>
{
    public void Configure(EntityTypeBuilder<RevokedToken> builder)
    {
        builder.ToTable("RevokedTokens");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Jti).IsRequired();
        builder.HasIndex(x => x.Jti).IsUnique();

        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.ExpiresAt).IsRequired();
        builder.Property(x => x.RevokedAt).IsRequired();
    }
}
