using TaskFlow.Platform.Domain.Users.Entities;
using TaskFlow.Platform.Domain.Users.Repositories;
using TaskFlow.Platform.Domain.Utils;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Platform.Persistence.Context;
using TaskFlow.Platform.Persistence.Utils;

namespace TaskFlow.Platform.Persistence.Users.Repositories;

public sealed class UserProfileRepository(ApiContext context) : IUserProfileRepository
{
    public Task<List<User>> GetAllAsync(CancellationToken cancellationToken)
    {
        return context.UserProfiles
            .AsNoTracking()
            .Include(x => x.Address)
            .OrderByDescending(x => x.Id)
            .ToListAsync(cancellationToken);
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return context.UserProfiles.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<User?> GetByIdWithRolesAsync(Guid id, CancellationToken cancellationToken)
    {
        return context.UserProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<User?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken)
    {
        return context.UserProfiles.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<User?> GetForUpdateWithAddressAsync(Guid id, CancellationToken cancellationToken)
    {
        return context.UserProfiles
            .Include(x => x.Address)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<string?> GetUserEmailAsync(Guid id, CancellationToken cancellationToken)
    {
        return await context.Users
            .AsNoTracking()
            .Where(u => u.Id == id)
            .Select(u => u.Email)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<User?> GetForUpdateWithRolesAsync(Guid id, CancellationToken cancellationToken)
    {
        return context.UserProfiles
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken)
    {
        await context.UserProfiles.AddAsync(user, cancellationToken);
    }

    public void Remove(User user)
    {
        context.UserProfiles.Remove(user);
    }

    public async Task<PagedList<User>> GetPagedAsync(int page, int pageSize, string? search, string? sortBy, bool? asc, CancellationToken cancellationToken)
    {
        var query = context.UserProfiles
            .AsNoTracking()
            .Include(x => x.Address)
            .Include(x => x.IdentityUser)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(u =>
                EF.Functions.ILike(u.FirstName, pattern) ||
                EF.Functions.ILike(u.LastName, pattern) ||
                (u.IdentityUser != null && u.IdentityUser.Email != null && EF.Functions.ILike(u.IdentityUser.Email, pattern)));
        }

        var ascending = asc ?? false;

        query = (sortBy?.ToLower()) switch
        {
            "firstname" => ascending ? query.OrderBy(u => u.FirstName) : query.OrderByDescending(u => u.FirstName),
            "lastname" => ascending ? query.OrderBy(u => u.LastName) : query.OrderByDescending(u => u.LastName),
            "email" => ascending
                ? query.OrderBy(u => u.IdentityUser != null ? u.IdentityUser.Email : string.Empty)
                : query.OrderByDescending(u => u.IdentityUser != null ? u.IdentityUser.Email : string.Empty),
            _ => query.OrderByDescending(u => u.CreatedAt),
        };

        return await PagedListConstructor<User>.CreateAsync(query, page, pageSize, cancellationToken);
    }
}
