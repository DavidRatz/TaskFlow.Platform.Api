using TaskFlow.Platform.Domain.Abstractions;
using TaskFlow.Platform.Domain.Auth.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using TaskFlow.Platform.Domain.Tasks.Entities;
using TaskFlow.Platform.Domain.Users.Entities;

namespace TaskFlow.Platform.Persistence.Context;

public sealed class ApiContext(DbContextOptions<ApiContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<User> UserProfiles => Set<User>();

    public DbSet<Address> Addresses => Set<Address>();

    public DbSet<TaskEmail> Tasks => Set<TaskEmail>();

    public DbSet<RevokedToken> RevokedTokens => Set<RevokedToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<RevokedToken>().HasQueryFilter(x => x.ExpiresAt > DateTimeOffset.UtcNow);

        builder.ApplyConfigurationsFromAssembly(AssemblyReference.Assembly);

        // Configure xmin as optimistic concurrency token for all domain entities (PostgreSQL only)
        if (Database.ProviderName == "Npgsql.EntityFrameworkCore.PostgreSQL")
        {
            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                if (typeof(Entity).IsAssignableFrom(entityType.ClrType))
                {
                    builder.Entity(entityType.ClrType)
                        .Property<uint>("RowVersion")
                        .HasColumnName("xmin")
                        .HasColumnType("xid")
                        .ValueGeneratedOnAddOrUpdate()
                        .IsConcurrencyToken();
                }
            }
        }
    }
}
