using TaskFlow.Platform.Domain.Authentication.Models;
using TaskFlow.Platform.Domain.Authentication.Services;
using Microsoft.IdentityModel.JsonWebTokens;

namespace TaskFlow.Platform.Infrastructure.Authentication.Services;

public sealed class LogoutTokenInfoProvider : ILogoutTokenInfoProvider
{
    public bool TryGet(Guid userId, string jti, string authorizationHeader, out LogoutTokenInfo tokenInfo)
    {
        tokenInfo = null!;

        if (string.IsNullOrWhiteSpace(jti))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(authorizationHeader)
            || !authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var rawToken = authorizationHeader["Bearer ".Length..].Trim();
        if (string.IsNullOrWhiteSpace(rawToken))
        {
            return false;
        }

        JsonWebToken parsed;
        try
        {
            parsed = new JsonWebTokenHandler().ReadJsonWebToken(rawToken);
        }
        catch (ArgumentException)
        {
            return false;
        }

        if (parsed.ValidTo == DateTime.MinValue)
        {
            return false;
        }

        tokenInfo = new LogoutTokenInfo(jti, userId, new DateTimeOffset(parsed.ValidTo, TimeSpan.Zero));
        return true;
    }
}
