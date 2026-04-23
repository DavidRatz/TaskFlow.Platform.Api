namespace TaskFlow.Platform.Application.Users.Dtos;

public sealed record AddressDto(
    Guid Id,
    string Street,
    string City,
    string PostalCode,
    string Country);
