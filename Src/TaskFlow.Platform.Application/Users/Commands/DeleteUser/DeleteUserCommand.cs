using MediatR;

namespace TaskFlow.Platform.Application.Users.Commands.DeleteUser;

public sealed record DeleteUserCommand(Guid UserId) : IRequest;
