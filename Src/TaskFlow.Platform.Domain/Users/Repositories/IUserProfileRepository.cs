using TaskFlow.Platform.Domain.Users.Entities;
using TaskFlow.Platform.Domain.Utils;

namespace TaskFlow.Platform.Domain.Users.Repositories;

public interface IUserProfileRepository : IGenericRepository<User>
{
    //// Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<User?> GetByIdWithRolesAsync(Guid id, CancellationToken cancellationToken);

    //// Task<User?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken);

    Task<User?> GetForUpdateWithAddressAsync(Guid id, CancellationToken cancellationToken);

    Task<string?> GetUserEmailAsync(Guid id, CancellationToken cancellationToken);

    Task<User?> GetForUpdateWithRolesAsync(Guid id, CancellationToken cancellationToken);

    //// Task AddAsync(User user, CancellationToken cancellationToken);

    Task<PagedList<User>> GetPagedAsync(int page, int pageSize, string? search, string? sortBy, bool? asc, CancellationToken cancellationToken);
}
