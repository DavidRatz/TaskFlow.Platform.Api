using System.Security.Claims;
using System.Text;
using TaskFlow.Platform.Domain.Auth.Entities;
using TaskFlow.Platform.Domain.Authentication.Models;
using TaskFlow.Platform.Domain.Authentication.Services;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using TaskFlow.Platform.Infrastructure.Authentication.Options;

namespace TaskFlow.Platform.Infrastructure.Authentication.Services;

public sealed class JwtTokenService(IOptions<JwtOptions> options) : IJwtTokenService
{
    private readonly JwtOptions _options = options.Value;

    public JwtTokenResult CreateToken(ApplicationUser user)
    {
        var now = DateTimeOffset.UtcNow;
        var expiresAt = now.AddDays(_options.ExpiresInDays);
        var jti = Guid.NewGuid().ToString("N");

        var claims = new List<Claim>
        {
            new (JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new (JwtRegisteredClaimNames.Jti, jti),
            new (JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new (ClaimTypes.NameIdentifier, user.Id.ToString()),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiresAt.UtcDateTime,
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            SigningCredentials = credentials,
        };

        var handler = new JsonWebTokenHandler();
        var token = handler.CreateToken(tokenDescriptor);

        return new JwtTokenResult(token, jti, expiresAt);
    }
}
