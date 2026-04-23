namespace TaskFlow.Platform.Domain.Authentication.Models;

public sealed record JwtTokenResult(string Token, string Jti, DateTimeOffset ExpiresAt);
