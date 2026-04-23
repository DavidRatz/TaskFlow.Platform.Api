namespace TaskFlow.Platform.Application.Tasks.Dtos;

public sealed record TaskDto(
    Guid Id,
    string Title,
    string? Description,
    Guid? UserId,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
