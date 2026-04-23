namespace TaskFlow.Platform.Domain.Authentication.Models;

public sealed record LogoutTokenInfo(string Jti, Guid UserId, DateTimeOffset ExpiresAt);
