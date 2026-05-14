using Marten.Pagination;

namespace Application.Mapping;

public record Page<T>
{
    public required IReadOnlyList<T> Items { get; init; }
    public required long PageNumber { get; init; }
    public required long PageSize { get; init; }
    public required long TotalItemCount { get; init; }
    public required long PageCount { get; init; }
    public required bool HasNextPage { get; init; }
    public required bool HasPreviousPage { get; init; }
}

public static class PagedListExtensions
{
    public static Page<T> ToPagedResponse<T>(this IPagedList<T> list)
    {
        return new Page<T>
        {
            Items = list.ToList(),
            PageNumber = list.PageNumber,
            PageSize = list.PageSize,
            TotalItemCount = list.TotalItemCount,
            PageCount = list.PageCount,
            HasNextPage = list.HasNextPage,
            HasPreviousPage = list.HasPreviousPage
        };
    }
}