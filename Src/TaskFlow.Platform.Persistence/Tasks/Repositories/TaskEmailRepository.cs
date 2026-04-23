using Microsoft.EntityFrameworkCore;
using TaskFlow.Platform.Domain.Tasks.Entities;
using TaskFlow.Platform.Domain.Tasks.Repositories;
using TaskFlow.Platform.Persistence.Context;

namespace TaskFlow.Platform.Persistence.Tasks.Repositories;

public sealed class TaskEmailRepository(ApiContext context) : ITaskEmailRepository
{
    public Task<List<TaskEmail>> GetAllAsync(CancellationToken cancellationToken)
    {
        return context.Tasks
            .AsNoTracking()
            .OrderByDescending(x => x.Id)
            .ToListAsync(cancellationToken);
    }

    public Task<TaskEmail?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken)
    {
        return context.Tasks.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task AddAsync(TaskEmail task, CancellationToken cancellationToken = default)
    {
        await context.Tasks.AddAsync(task, cancellationToken);
    }

    public void Remove(TaskEmail entity)
    {
        context.Tasks.Remove(entity);
    }

    public Task<TaskEmail?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return context.Tasks.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }
}
