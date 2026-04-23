using MediatR;
using TaskFlow.Platform.Application.Users.Dtos;

namespace TaskFlow.Platform.Application.Users.Commands.UpdateAuthenticatedUserAddressCommand;

public sealed record UpdateAuthenticatedUserAddressCommand(
    Guid UserId,
    string? Street,
    string? City,
    string? PostalCode,
    string? Country) : IRequest<AddressDto>;
