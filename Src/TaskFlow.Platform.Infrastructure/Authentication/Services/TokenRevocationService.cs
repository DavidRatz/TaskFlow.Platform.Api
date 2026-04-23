using TaskFlow.Platform.Domain.Auth.Entities;
using TaskFlow.Platform.Domain.Authentication.Services;
using TaskFlow.Platform.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace TaskFlow.Platform.Infrastructure.Authentication.Services;

public sealed class TokenRevocationService(ApiContext context) : ITokenRevocationService
{
    public Task<bool> IsRevokedAsync(string jti, CancellationToken cancellationToken)
    {
        return context.RevokedTokens.AsNoTracking().AnyAsync(x => x.Jti == jti, cancellationToken);
    }

    public async Task RevokeAsync(string jti, Guid userId, DateTimeOffset expiresAt, CancellationToken cancellationToken)
    {
        var exists = await context.RevokedTokens.AnyAsync(x => x.Jti == jti, cancellationToken);
        if (exists)
        {
            return;
        }

        context.RevokedTokens.Add(new RevokedToken
        {
            Id = Guid.NewGuid(),
            Jti = jti,
            UserId = userId,
            ExpiresAt = expiresAt,
            RevokedAt = DateTimeOffset.UtcNow,
        });

        await context.SaveChangesAsync(cancellationToken);
    }
}
