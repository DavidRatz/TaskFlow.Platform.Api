using System.Globalization;
using TaskFlow.Platform.Application;
using TaskFlow.Platform.Infrastructure;
using TaskFlow.Platform.Persistence;
using TaskFlow.Platform.Persistence.Context;
using TaskFlow.Platform.Persistence.Extensions;
using TaskFlow.Platform.Presentation;
using Carter;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

var customTheme = new AnsiConsoleTheme(new Dictionary<ConsoleThemeStyle, string>
{
    [ConsoleThemeStyle.Text] = "\x1b[38;5;253m",
    [ConsoleThemeStyle.SecondaryText] = "\x1b[38;5;246m",
    [ConsoleThemeStyle.TertiaryText] = "\x1b[38;5;242m",
    [ConsoleThemeStyle.Invalid] = "\x1b[33;1m",
    [ConsoleThemeStyle.Null] = "\x1b[38;5;38m",
    [ConsoleThemeStyle.Name] = "\x1b[38;5;81m",
    [ConsoleThemeStyle.String] = "\x1b[38;5;216m",
    [ConsoleThemeStyle.Number] = "\x1b[38;5;151m",
    [ConsoleThemeStyle.Boolean] = "\x1b[38;5;38m",
    [ConsoleThemeStyle.Scalar] = "\x1b[38;5;79m",
    [ConsoleThemeStyle.LevelVerbose] = "\x1b[38;5;242m",
    [ConsoleThemeStyle.LevelDebug] = "\x1b[38;5;247m",
    [ConsoleThemeStyle.LevelInformation] = "\x1b[38;5;40m",
    [ConsoleThemeStyle.LevelWarning] = "\x1b[38;5;226m\x1b[1m",
    [ConsoleThemeStyle.LevelError] = "\x1b[38;5;196m\x1b[48;5;52m\x1b[1m",
    [ConsoleThemeStyle.LevelFatal] = "\x1b[38;5;231m\x1b[48;5;196m\x1b[1m",
});

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Service", "API")
    .WriteTo.Console(
        theme: customTheme,
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
        formatProvider: CultureInfo.InvariantCulture)
    .CreateLogger();

builder.Host.UseSerilog();

try
{
    Log.Information("=== TaskFlow API ===");
    Log.Information("Environment: {Environment}", builder.Environment.EnvironmentName);

    var appUrl = builder.Configuration["AppUrl"] ?? "https://taskflow.valentinlopez.pro";

    builder.Services.AddOpenApi(options =>
    {
        options.AddDocumentTransformer((document, _, _) =>
        {
            document.Info = new OpenApiInfo
            {
                Title = "TaskFlow API",
                Version = "v1",
                Description = "API for managing car wash operations, stations, subscriptions, vehicles, and wash sessions.",
                Contact = new OpenApiContact
                {
                    Name = "TaskFlow",
                    Url = new Uri(appUrl),
                },
            };

            var components = document.Components ??= new OpenApiComponents();
            components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
            components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Enter your JWT token",
            };

            (document.Security ??= []).Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = [],
            });

            return Task.CompletedTask;
        });
    });
    builder.Services.AddAuthorization();

    var corsOrigins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>()
                      ?? ["http://localhost:3000"];

    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.AllowAnyHeader().AllowAnyMethod();

            if (corsOrigins.Contains("*"))
            {
                policy.AllowAnyOrigin();
            }
            else
            {
                policy.WithOrigins(corsOrigins).AllowCredentials();
            }
        });
    });

    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddPersistence(builder.Configuration);
    builder.Services.AddApplication(builder.Configuration);
    builder.Services.AddPresentation(builder.Configuration);

    var app = builder.Build();

    if (!app.Configuration.GetValue<bool>("SkipDatabaseMigrations"))
    {
        await UpdateDatabase(app);
    }

    if (app.Configuration.GetValue<bool>("SeedDatabase"))
    {
        await app.SeedDatabaseAsync();
    }

    var forwardedHeadersOptions = new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
    };
    forwardedHeadersOptions.KnownIPNetworks.Clear();
    forwardedHeadersOptions.KnownProxies.Clear();
    app.UseForwardedHeaders(forwardedHeadersOptions);

    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate =
            "{RequestMethod} {RequestPath} -> {StatusCode} in {Elapsed:0.00}ms";
        options.GetLevel = (httpContext, elapsed, ex) =>
            ex != null ? LogEventLevel.Error
            : httpContext.Response.StatusCode >= 500 ? LogEventLevel.Error
            : httpContext.Response.StatusCode >= 400 ? LogEventLevel.Warning
            : LogEventLevel.Information;
    });

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference(options =>
        {
            options
                .WithTitle("TaskFlow API")
                .WithTheme(ScalarTheme.Saturn)
                .WithDefaultHttpClient(ScalarTarget.JavaScript, ScalarClient.Fetch)
                .AddPreferredSecuritySchemes(["Bearer"])
                .AddHttpAuthentication("Bearer", scheme =>
                {
                    scheme.Token = string.Empty;
                })
                .WithPersistentAuthentication(true);
        });
    }

    app.UseCors();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapCarter();

    Log.Information("API listening");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "API terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}

return;

static async Task UpdateDatabase(IApplicationBuilder app)
{
    using var serviceScope = app.ApplicationServices.CreateScope();
    var context = serviceScope.ServiceProvider.GetRequiredService<ApiContext>();
    await context.Database.MigrateAsync();
}
