using System.Collections.Generic;

namespace Source.Shared
{
    public sealed record PageResponse<T>
    {
        public IReadOnlyList<T> Items { get; set; }
        public long PageNumber { get; set; }
        public long PageSize { get; set; }
        public long TotalItemCount { get; set; }
        public long PageCount { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }
}