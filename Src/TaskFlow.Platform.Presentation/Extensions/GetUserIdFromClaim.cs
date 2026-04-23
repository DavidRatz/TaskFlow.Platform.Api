using System.Security.Claims;
using TaskFlow.Platform.Application.Common.Exceptions;

namespace TaskFlow.Platform.Presentation.Extensions;

public static class GetUserIdFromClaim
{
    public static Guid GetUserId(this ClaimsPrincipal claim)
    {
        var userIdValue = claim.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? claim.FindFirstValue("sub");

        return !Guid.TryParse(userIdValue, out var userId) ? throw new UnauthorizedAuthException() : userId;
    }

    public static string GetJti(this ClaimsPrincipal claim)
    {
        var jti = claim.FindFirstValue("jti");
        return string.IsNullOrWhiteSpace(jti) ? throw new UnauthorizedAuthException() : jti;
    }
}
