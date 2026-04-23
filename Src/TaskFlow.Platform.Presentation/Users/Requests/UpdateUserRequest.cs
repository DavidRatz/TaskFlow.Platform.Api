namespace TaskFlow.Platform.Presentation.Users.Requests;

public sealed record UpdateUserRequest(
    string? FirstName,
    string? LastName,
    string? Phone,
    string? LegalName,
    string? VatNumber,
    UpdateUserAddressRequest? Address);

public sealed record UpdateUserAddressRequest(
    string? Street,
    string? City,
    string? PostalCode,
    string? Country);
