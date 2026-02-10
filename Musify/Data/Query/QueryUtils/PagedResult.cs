namespace Musify.Data.Query.QueryUtils
{
    public class PagedResult<T>
    {
        public IReadOnlyList<T> Items { get; init; } = [];
        public int TotalCount { get; init; }
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 10;
    }
}