using Musify.Data.Query.QueryUtils;
using Musify.Dtos;

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