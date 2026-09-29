namespace CulinaryBlog.Application.Common.Models;

public sealed record ApiResponse<T>(T Data);

public sealed record ApiPagedResponse<T>(
    IReadOnlyList<T> Data,
    PaginationMeta Meta);

public sealed record PaginationMeta(
    int Page,
    int PageSize,
    int Total,
    int TotalPages,
    bool HasNextPage,
    bool HasPreviousPage);
