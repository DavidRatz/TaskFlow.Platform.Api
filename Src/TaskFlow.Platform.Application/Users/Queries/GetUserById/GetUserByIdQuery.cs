using MediatR;
using TaskFlow.Platform.Application.Users.Dtos;

namespace TaskFlow.Platform.Application.Users.Queries.GetUserById;

public sealed record GetUserByIdQuery(Guid UserId) : IRequest<UserDto>;
