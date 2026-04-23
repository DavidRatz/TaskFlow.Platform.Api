namespace TaskFlow.Platform.Presentation.Me.Requests;

public sealed record CreateAddressRequest(
    string Street,
    string City,
    string PostalCode,
    string Country);

public sealed record UpdateAddressRequest(
    string? Street,
    string? City,
    string? PostalCode,
    string? Country);
