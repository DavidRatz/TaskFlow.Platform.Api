using MediatR;
using TaskFlow.Platform.Application.Users.Dtos;

namespace TaskFlow.Platform.Application.Users.Commands.UpdateMe;

public sealed record UpdateMeCommand(
    Guid UserId,
    string? FirstName,
    string? LastName,
    string? Phone,
    string? LegalName,
    string? VatNumber,
    UpdateAuthenticatedUserAddressDto? Address) : IRequest<UserDto>;

public sealed record UpdateAuthenticatedUserAddressDto(
    string? Street,
    string? City,
    string? PostalCode,
    string? Country);
