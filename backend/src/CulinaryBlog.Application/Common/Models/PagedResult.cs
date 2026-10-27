namespace CulinaryBlog.Application.Common.Models;

public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; }
        = Array.Empty<T>();

    public int TotalCount { get; init; }

    public int Page { get; init; }

    public int PageSize { get; init; }

    public int TotalPages { get; init; }

    public bool HasNextPage =>
        Page < TotalPages;

    public bool HasPreviousPage =>
        Page > 1;

    public PagedResult(
        IReadOnlyList<T> items,
        int totalCount,
        int page,
        int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        Page = page;
        PageSize = pageSize;

        TotalPages = pageSize == 0
            ? 0
            : (int)Math.Ceiling(
                totalCount / (double)pageSize);
    }
}