using TaskFlow.Platform.Domain.Abstractions;
using Microsoft.Extensions.Logging;
using TaskFlow.Platform.Persistence.Seeding;

namespace TaskFlow.Platform.Persistence.Seeders;

public sealed class DatabaseSeeder(
    UserSeeder userSeeder,
    ILogger<DatabaseSeeder> logger)
    : ISeeder
{
    public async Task SeedAsync()
    {
        try
        {
            logger.LogInformation("Starting database seeding...");

            logger.LogInformation("Seeding users...");
            await userSeeder.SeedAsync();

            logger.LogInformation("Database seeding completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }
}
