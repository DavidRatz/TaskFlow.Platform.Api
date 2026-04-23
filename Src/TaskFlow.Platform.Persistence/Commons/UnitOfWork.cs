using TaskFlow.Platform.Domain.Abstractions;
using TaskFlow.Platform.Domain.Commons;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Platform.Persistence.Context;

namespace TaskFlow.Platform.Persistence.Commons;

public class UnitOfWork(ApiContext context) : IUnitOfWork
{
    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateCreatedAndUpdatedDate();
        return context.SaveChangesAsync(cancellationToken);
    }

    private void UpdateCreatedAndUpdatedDate()
    {
        var currentTime = DateTime.UtcNow;

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.Entity is Entity entity)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entity.CreatedAt = currentTime;
                        entity.UpdatedAt = currentTime;
                        break;

                    case EntityState.Modified:
                        entry.Property(nameof(Entity.CreatedAt)).IsModified = false;
                        entity.UpdatedAt = currentTime;
                        break;
                }
            }
        }
    }
}
