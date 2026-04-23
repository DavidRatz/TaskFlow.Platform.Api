namespace TaskFlow.Platform.Presentation.Me.Requests;

public sealed record UpdateMeRequest(
    string? FirstName,
    string? LastName,
    string? Phone,
    string? LegalName,
    string? VatNumber,
    UpdateMeAddressRequest? Address);

public sealed record UpdateMeAddressRequest(
    string? Street,
    string? City,
    string? PostalCode,
    string? Country);
