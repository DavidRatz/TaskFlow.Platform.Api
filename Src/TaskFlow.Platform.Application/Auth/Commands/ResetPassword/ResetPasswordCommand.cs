using MediatR;

namespace TaskFlow.Platform.Application.Auth.Commands.ResetPassword;

public sealed record ResetPasswordCommand(string Token, string Password) : IRequest;
