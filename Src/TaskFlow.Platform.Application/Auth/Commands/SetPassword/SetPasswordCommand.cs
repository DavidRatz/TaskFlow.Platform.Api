using MediatR;

namespace TaskFlow.Platform.Application.Auth.Commands.SetPassword;

public sealed record SetPasswordCommand(string Token, string Password) : IRequest;
