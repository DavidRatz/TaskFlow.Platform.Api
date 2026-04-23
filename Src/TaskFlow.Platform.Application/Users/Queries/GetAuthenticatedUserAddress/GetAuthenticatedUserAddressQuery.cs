using MediatR;
using TaskFlow.Platform.Application.Users.Dtos;

namespace TaskFlow.Platform.Application.Users.Queries.GetAuthenticatedUserAddress;

public sealed record GetAuthenticatedUserAddressQuery(Guid UserId) : IRequest<AddressDto>;
