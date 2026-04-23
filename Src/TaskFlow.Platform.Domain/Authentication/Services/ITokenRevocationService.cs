namespace TaskFlow.Platform.Domain.Authentication.Services;

public interface ITokenRevocationService
{
    Task<bool> IsRevokedAsync(string jti, CancellationToken cancellationToken);

    Task RevokeAsync(string jti, Guid userId, DateTimeOffset expiresAt, CancellationToken cancellationToken);
}
