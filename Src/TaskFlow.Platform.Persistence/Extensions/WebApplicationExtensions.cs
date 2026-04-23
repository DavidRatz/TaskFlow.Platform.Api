using TaskFlow.Platform.Domain.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace TaskFlow.Platform.Persistence.Extensions;

public static class WebApplicationExtensions
{
    public static async Task SeedDatabaseAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<IApplicationBuilder>>();

        try
        {
            var seeder = services.GetRequiredService<ISeeder>();
            await seeder.SeedAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }
}
