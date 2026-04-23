namespace TaskFlow.Platform.Domain.Emails.Models;

public sealed class WelcomeMail
{
    public required string FirstName { get; set; }

    public required string LastName { get; set; }

    public required string Email { get; set; }
}
