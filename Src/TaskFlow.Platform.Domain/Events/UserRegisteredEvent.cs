namespace TaskFlow.Platform.Domain.Events;

public sealed class UserRegisteredEvent
{
    public required Guid UserId { get; init; }

    public required string Email { get; init; }

    public required string FirstName { get; init; }

    public required string LastName { get; init; }

    public string? Phone { get; init; }

    public string? LegalName { get; init; }

    public string? VatNumber { get; init; }

    public UserRegisteredAddressDto? Address { get; init; }
}

public sealed record UserRegisteredAddressDto(
    string Street,
    string City,
    string PostalCode,
    string Country);
