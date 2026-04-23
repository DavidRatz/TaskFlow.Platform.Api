namespace TaskFlow.Platform.Domain.Commons;

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
