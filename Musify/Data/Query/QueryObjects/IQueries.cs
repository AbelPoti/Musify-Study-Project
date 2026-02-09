using Musify.Data.Query.QueryUtils;

namespace Musify.Data.Query.QueryObjects
{
    public interface IQueries<T>
    {
        Task<PagedResult<T>> GetItemsAsync(
            PageRequest pageRequest,
            SortRequest sortRequest,
            CancellationToken cancellationToken);
    }
}