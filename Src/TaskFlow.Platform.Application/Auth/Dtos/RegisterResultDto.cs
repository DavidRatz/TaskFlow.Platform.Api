using TaskFlow.Platform.Application.Users.Dtos;

namespace TaskFlow.Platform.Application.Auth.Dtos;

public sealed record RegisterResultDto(UserDto User, string Token);
