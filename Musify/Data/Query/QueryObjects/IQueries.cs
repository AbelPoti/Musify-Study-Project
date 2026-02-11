using Musify.Data.Query.QueryUtils;
using Musify.Dtos.RequestDtos;

namespace Musify.Data.Query.QueryObjects
{
    public interface IQueries<TEntity, in TFilter>
    {
        Task<PagedResult<TEntity>> GetItemsAsync(
            PageRequest pageRequest,
            SortRequest? sortRequest,
            TFilter? filter,
            CancellationToken cancellationToken);
    }
}