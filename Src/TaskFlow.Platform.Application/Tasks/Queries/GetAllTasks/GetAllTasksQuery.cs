using MediatR;
using TaskFlow.Platform.Application.Tasks.Dtos;

namespace TaskFlow.Platform.Application.Tasks.Queries.GetAllTasks;

public sealed record GetAllTasksQuery() : IRequest<IReadOnlyList<TaskDto>>;
