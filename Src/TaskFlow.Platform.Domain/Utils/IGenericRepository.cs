namespace TaskFlow.Platform.Domain.Utils;

public interface IGenericRepository<T>
    where T : class
{
    Task<List<T>> GetAllAsync(CancellationToken cancellationToken);

    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<T?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken);

    Task AddAsync(T entity, CancellationToken cancellationToken);

    void Remove(T entity);
}
