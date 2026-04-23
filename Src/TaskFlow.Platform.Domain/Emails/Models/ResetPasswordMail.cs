namespace TaskFlow.Platform.Domain.Emails.Models;

public sealed class ResetPasswordMail
{
    public required string FirstName { get; set; }

    public required string ResetUrl { get; set; }
}
