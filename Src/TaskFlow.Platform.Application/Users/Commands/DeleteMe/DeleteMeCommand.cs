using MediatR;

namespace TaskFlow.Platform.Application.Users.Commands.DeleteMe;

public sealed record DeleteMeCommand(Guid UserId, string Jti, string AuthorizationHeader) : IRequest;
