using MediatR;
using TaskFlow.Platform.Application.Users.Dtos;

namespace TaskFlow.Platform.Application.Users.Queries.GetMe;

public sealed record GetMeQuery(Guid UserId) : IRequest<UserDto>;
