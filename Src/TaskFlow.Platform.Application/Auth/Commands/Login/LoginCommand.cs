using MediatR;
using TaskFlow.Platform.Application.Users.Dtos;

namespace TaskFlow.Platform.Application.Auth.Commands.Login;

public sealed record LoginCommand(string Email, string Password) : IRequest<LoginResult>;

public sealed record LoginResult(UserDto User, string Token);
