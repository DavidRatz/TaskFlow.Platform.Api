namespace TaskFlow.Platform.Domain.Tasks.Entities;

public sealed class TaskEmail : Abstractions.Entity
{
    public required string Title { get; set; }

    public string? Description { get; set; }

    public Guid? UserId { get; set; }

    // Add other properties as needed, e.g., Status, DueDate, etc.
}
