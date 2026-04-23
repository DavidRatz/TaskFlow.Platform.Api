using TaskFlow.Platform.Domain.Authentication.Models;

namespace TaskFlow.Platform.Domain.Authentication.Services;

public interface ILogoutTokenInfoProvider
{
    bool TryGet(Guid userId, string jti, string authorizationHeader, out LogoutTokenInfo tokenInfo);
}
