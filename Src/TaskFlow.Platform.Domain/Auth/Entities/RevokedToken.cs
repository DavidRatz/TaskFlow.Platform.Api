namespace TaskFlow.Platform.Domain.Auth.Entities;

public sealed class RevokedToken
{
    public Guid Id { get; set; }

    public string Jti { get; set; } = string.Empty;

    public Guid UserId { get; set; }

    public DateTimeOffset ExpiresAt { get; set; }

    public DateTimeOffset RevokedAt { get; set; }
}
