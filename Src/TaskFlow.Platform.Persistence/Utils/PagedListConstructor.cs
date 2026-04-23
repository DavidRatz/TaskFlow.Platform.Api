using TaskFlow.Platform.Domain.Utils;
using Microsoft.EntityFrameworkCore;

namespace TaskFlow.Platform.Persistence.Utils;

public static class PagedListConstructor<T>
{
    public static async Task<PagedList<T>> CreateAsync(IQueryable<T> query, int page, int pageSize,
        CancellationToken cancellationToken)
    {
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return new PagedList<T>(items, page, pageSize, totalCount);
    }
}
