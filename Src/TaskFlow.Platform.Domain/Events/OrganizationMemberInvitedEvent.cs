namespace TaskFlow.Platform.Domain.Events;

public sealed class OrganizationMemberInvitedEvent
{
    public required Guid OrganizationId { get; init; }

    public required string OrganizationName { get; init; }

    public required Guid UserId { get; init; }

    public required string Email { get; init; }

    public required string FirstName { get; init; }

    public required bool IsNewUser { get; init; }

    public string? TemporaryPassword { get; init; }
}
