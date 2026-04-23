namespace TaskFlow.Platform.Domain.Emails.Models;

public sealed class OrganizationInvitationMail
{
    public required string OrganizationName { get; set; }

    public required string InviteeEmail { get; set; }

    public required string InviteeFirstName { get; set; }

    public required bool IsNewUser { get; set; }

    public string? TemporaryPassword { get; set; }
}
