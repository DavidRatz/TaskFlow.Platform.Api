using System.Text;
using TaskFlow.Platform.Domain.Authentication.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using TaskFlow.Platform.Infrastructure.Authentication.Options;

namespace TaskFlow.Platform.Infrastructure.Authentication.Extensions;

public static class JwtAuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var jwt = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1),
                };

                options.MapInboundClaims = false;

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var jti = context.Principal?.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti)?.Value;
                        if (string.IsNullOrWhiteSpace(jti))
                        {
                            context.Fail("Missing jti");
                            return;
                        }

                        var tokenRevocationService = context.HttpContext.RequestServices.GetRequiredService<ITokenRevocationService>();
                        var isRevoked = await tokenRevocationService.IsRevokedAsync(jti, context.HttpContext.RequestAborted);
                        if (isRevoked)
                        {
                            context.Fail("Token revoked");
                        }
                    },
                };
            });

        services.AddAuthorization();

        return services;
    }
}
