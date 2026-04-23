using MediatR;

namespace TaskFlow.Platform.Application.Users.Commands.DeleteAuthenticatedUserAddressCommand;

public sealed record DeleteAuthenticatedUserAddressCommand(Guid UserId) : IRequest;
