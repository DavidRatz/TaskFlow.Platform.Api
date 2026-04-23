using MediatR;
using TaskFlow.Platform.Application.Users.Dtos;

namespace TaskFlow.Platform.Application.Users.Commands.CreateAuthenticatedUserAddress;

public sealed record CreateAuthenticatedUserAddressCommand(
    Guid UserId,
    string Street,
    string City,
    string PostalCode,
    string Country) : IRequest<AddressDto>;
