using TaskFlow.Platform.Domain.Auth.Entities;
using TaskFlow.Platform.Domain.Authentication.Models;

namespace TaskFlow.Platform.Domain.Authentication.Services;

public interface IJwtTokenService
{
    JwtTokenResult CreateToken(ApplicationUser user);
}
