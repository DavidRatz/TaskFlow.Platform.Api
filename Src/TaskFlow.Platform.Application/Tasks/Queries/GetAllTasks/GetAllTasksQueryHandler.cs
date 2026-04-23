using MediatR;
using TaskFlow.Platform.Application.Tasks.Dtos;
using TaskFlow.Platform.Domain.Tasks.Repositories;

namespace TaskFlow.Platform.Application.Tasks.Queries.GetAllTasks;

public sealed class GetAllTasksQueryHandler(
    ITaskEmailRepository taskEmailRepository
    ) : IRequestHandler<GetAllTasksQuery, IReadOnlyList<TaskDto>>
{
    public async Task<IReadOnlyList<TaskDto>> Handle(GetAllTasksQuery request, CancellationToken cancellationToken)
    {
        var tasks = await taskEmailRepository.GetAllAsync(cancellationToken);

        return [.. tasks
            .Select(t => new TaskDto(
                t.Id,
                t.Title,
                t.Description,
                t.UserId,
                t.CreatedAt,
                t.UpdatedAt))];
    }
}
