namespace TaskFlow.Platform.Application.Users.Dtos;

public sealed record UserOrganizationDto(Guid Id, string Name, string Role);

public sealed record UserDto(
    Guid Id,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string? Email,
    string FirstName,
    string LastName,
    string? Phone,
    Guid? AddressId,
    AddressDto? Address,
    string? LegalName,
    string? VatNumber);
