namespace TaskFlow.Platform.Presentation.Auth.Requests;

public sealed record RegisterAddressRequest(string Street, string City, string PostalCode, string Country);
