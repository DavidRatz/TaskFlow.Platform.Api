using TaskFlow.Platform.Domain.Users.Entities;

namespace TaskFlow.Platform.Domain.Users.Repositories;

public interface IAddressRepository
{
    Task AddAsync(Address address, CancellationToken cancellationToken);

    Task<Address?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Address?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken);

    void Remove(Address address);
}
