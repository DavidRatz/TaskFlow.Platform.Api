using MediatR;

namespace TaskFlow.Platform.Application.Auth.Commands.Logout;

public sealed record LogoutCommand(Guid UserId, string Jti, string AuthorizationHeader) : IRequest;
