using MediatR;
using TaskFlow.Platform.Application.Users.Dtos;

namespace TaskFlow.Platform.Application.Users.Commands.CreateUser;

public sealed record CreateUserCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? Type,
    CreateUserAddressDto? Address) : IRequest<UserDto>;

public sealed record CreateUserAddressDto(
    string Street,
    string City,
    string PostalCode,
    string Country);
