using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace TaskFlow.Platform.Persistence.Context;

public sealed class ApiContextFactory : IDesignTimeDbContextFactory<ApiContext>
{
    public ApiContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=carwashflow;Username=postgres;Password=postgres";

        var optionsBuilder = new DbContextOptionsBuilder<ApiContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new ApiContext(optionsBuilder.Options);
    }
}
