namespace TaskFlow.Platform.Presentation.Auth.Requests;

public sealed record RegisterRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? Phone,
    RegisterAddressRequest Address,
    string? LegalName,
    string? VatNumber);
