using MediatR;
using TaskFlow.Platform.Application.Auth.Dtos;

namespace TaskFlow.Platform.Application.Auth.Commands.Register;

public sealed record RegisterCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? Phone,
    RegisterAddressDto Address,
    string? LegalName,
    string? VatNumber) : IRequest<RegisterResultDto>;
