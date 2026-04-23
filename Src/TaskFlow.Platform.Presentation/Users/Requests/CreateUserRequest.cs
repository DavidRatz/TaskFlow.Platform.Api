namespace TaskFlow.Platform.Presentation.Users.Requests;

public sealed record CreateUserRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? Type,
    CreateUserAddressRequest? Address);

public sealed record CreateUserAddressRequest(
    string Street,
    string City,
    string PostalCode,
    string Country);
