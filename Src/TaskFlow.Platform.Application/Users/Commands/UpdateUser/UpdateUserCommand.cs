using MediatR;
using TaskFlow.Platform.Application.Users.Dtos;

namespace TaskFlow.Platform.Application.Users.Commands.UpdateUser;

public sealed record UpdateUserCommand(
    Guid UserId,
    string? FirstName,
    string? LastName,
    string? Phone,
    string? LegalName,
    string? VatNumber,
    UpdateUserAddressDto? Address) : IRequest<UserDto>;

public sealed record UpdateUserAddressDto(
    string? Street,
    string? City,
    string? PostalCode,
    string? Country);
