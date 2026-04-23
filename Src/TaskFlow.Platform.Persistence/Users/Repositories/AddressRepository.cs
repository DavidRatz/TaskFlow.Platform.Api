using TaskFlow.Platform.Domain.Users.Entities;
using TaskFlow.Platform.Domain.Users.Repositories;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Platform.Persistence.Context;

namespace TaskFlow.Platform.Persistence.Users.Repositories;

public sealed class AddressRepository(ApiContext context) : IAddressRepository
{
    public async Task AddAsync(Address address, CancellationToken cancellationToken)
    {
        await context.Addresses.AddAsync(address, cancellationToken);
    }

    public Task<Address?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return context.Addresses.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<Address?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken)
    {
        return context.Addresses.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public void Remove(Address address)
    {
        context.Addresses.Remove(address);
    }
}
