using MediatR;

namespace TaskFlow.Platform.Application.Auth.Commands.ForgotPassword;

public sealed record ForgotPasswordCommand(string Email) : IRequest;
