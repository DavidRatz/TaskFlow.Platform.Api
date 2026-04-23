using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Platform.Domain.Auth.Entities;
using TaskFlow.Platform.Domain.Commons;
using TaskFlow.Platform.Domain.Users.Repositories;
using TaskFlow.Platform.Domain.Abstractions;
using TaskFlow.Platform.Domain.Tasks.Repositories;
using TaskFlow.Platform.Persistence.Commons;
using TaskFlow.Platform.Persistence.Context;
using TaskFlow.Platform.Persistence.Seeders;
using TaskFlow.Platform.Persistence.Seeding;
using TaskFlow.Platform.Persistence.Seeding.Options;
using TaskFlow.Platform.Persistence.Tasks.Repositories;
using TaskFlow.Platform.Persistence.Users.Repositories;

namespace TaskFlow.Platform.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApiContext>(x =>
            x.UseNpgsql(configuration.GetConnectionString("DefaultConnection")!));

        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;

                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
            })
            .AddEntityFrameworkStores<ApiContext>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IAddressRepository, AddressRepository>();
        services.AddScoped<IUserProfileRepository, UserProfileRepository>();
        services.AddScoped<ITaskEmailRepository, TaskEmailRepository>();

        services.AddOptions<AdminSeedOptions>()
            .Bind(configuration.GetSection(AdminSeedOptions.SectionName));

        services.AddScoped<ISeeder, DatabaseSeeder>();
        services.AddScoped<UserSeeder>();

        return services;
    }
}
