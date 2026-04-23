using MediatR;
using TaskFlow.Platform.Application.Tasks.Dtos;

namespace TaskFlow.Platform.Application.Tasks.Commands;

public sealed record CreateTaskFromEmailCommand(
    string Subject,
    string Message,
    string Email
) : IRequest<TaskDto>;
