namespace TaskFlow.Platform.Domain.Utils;

public sealed class PagedList<T>(IList<T> data, int page, int pageSize, int totalCount)
{
    public IList<T> Data { get; } = data;

    public int Page { get; } = page;

    public int PageSize { get; } = pageSize;

    public int TotalCount { get; } = totalCount;

    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    public bool HasNextPage => Page * PageSize < TotalCount;

    public bool HasPreviousPage => Page > 1;
}
