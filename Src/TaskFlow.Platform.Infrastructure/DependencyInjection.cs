using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskFlow.Platform.Domain.Authentication.Services;
using TaskFlow.Platform.Domain.Emails.Services;
using TaskFlow.Platform.Infrastructure.Authentication.Extensions;
using Microsoft.AspNetCore.Mvc.Razor;
using TaskFlow.Platform.Domain.Abstractions;
using TaskFlow.Platform.Infrastructure.Authentication.Options;
using TaskFlow.Platform.Infrastructure.Authentication.Services;
using TaskFlow.Platform.Infrastructure.Common;
using TaskFlow.Platform.Infrastructure.Emails.Options;
using TaskFlow.Platform.Infrastructure.Emails.Services;

namespace TaskFlow.Platform.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(o => !string.IsNullOrWhiteSpace(o.Secret) && o.Secret.Length >= 32)
            .ValidateOnStart();

        services.AddJwtAuthentication(configuration);

        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<ILogoutTokenInfoProvider, LogoutTokenInfoProvider>();
        services.AddScoped<ITokenRevocationService, TokenRevocationService>();

        // mail config
        services.AddOptions<EmailOptions>()
            .Bind(configuration.GetSection(EmailOptions.SectionName));

        services.AddOptions<MailSecretOptions>()
            .Bind(configuration.GetSection(MailSecretOptions.SectionName));

        services.AddScoped<ISendEmailService, SendEmailService>();
        services.AddScoped<IEmailService, RazorEmailService>();
        services.AddOptions<MailServerOptions>()
            .Bind(configuration.GetSection(MailServerOptions.SectionName));

        services.AddScoped<IDateTimeProvider, DateTimeProvider>();

        services.AddScoped<ITokenProvider, GmailTokenProvider>();
        services.AddScoped<ITokenProvider, OutlookTokenProvider>();

        services.AddScoped<ITokenProviderFactory, TokenProviderFactory>();

        services.AddScoped<IEmailConnector, EmailConnector>();

        // Add MVC core services required for Razor
        services.AddMvcCore()
            .AddRazorViewEngine()
            .AddRazorRuntimeCompilation();

        // Configure view locations
        services.Configure<RazorViewEngineOptions>(options =>
        {
            options.ViewLocationFormats.Add("/Views/{0}.cshtml");
            options.ViewLocationFormats.Add("/EmailTemplates/{0}.cshtml");
        });
        return services;
    }
}
