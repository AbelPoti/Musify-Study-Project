namespace Musify.Data.Query.QueryUtils
{
    public class PagedResult<T>
    {
        public IReadOnlyList<T> Items { get; init; }
        public int TotalCount { get; init; }
        public int Page { get; init; }
        public int PageSize { get; init; }
        
        public PagedResult(IReadOnlyList<T> items, int totalCount, int page = 1, int pageSize = 10)
        {
            Items = items;
            TotalCount = totalCount;
            Page = page;
            PageSize = pageSize;
        }
        
        public PagedResult<TResult> Map<TResult>(Func<T, TResult> selector)
        {
            return new PagedResult<TResult>(
                Items.Select(selector).ToList(),
                TotalCount,
                Page,
                PageSize);
        }
    }
}