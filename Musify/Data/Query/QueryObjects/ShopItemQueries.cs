using Microsoft.EntityFrameworkCore;
using Musify.Data.DatabaseContext;
using Musify.Data.Query.QueryUtils;
using Musify.Dtos;
using Musify.Models;

namespace Musify.Data.Query.QueryObjects
{
    public class ShopItemQueries : IQueries<ShopItem>
    {
        private readonly MusifyDbContext _dbContext;
        
        public ShopItemQueries(MusifyDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        
        public async Task<PagedResult<ShopItem>> GetItemsAsync(
            PageRequest pageRequest,
            SortRequest sortRequest,
            CancellationToken cancellationToken)
        {
            IQueryable<ShopItem> query = _dbContext.ShopItems.AsNoTracking();
            
            // Sorting - explicit mapping with fallback
            query = sortRequest.SortBy switch
            {
                "instrumentId" => sortRequest.Descending
                    ? query.OrderByDescending(sI => sI.InstrumentId)
                    : query.OrderBy(sI => sI.InstrumentId),
                
                "price" => sortRequest.Descending
                    ? query.OrderByDescending(sI => sI.Price)
                    : query.OrderBy(sI => sI.Price),
                
                "condition" => sortRequest.Descending
                    ? query.OrderByDescending(sI => sI.Condition)
                    : query.OrderBy(sI => sI.Condition),
                
                _ => query.OrderBy(sI => sI.Id)
            };
            
            int totalCount = await query.CountAsync(cancellationToken);
            
            List<ShopItem> items = await query
                .Skip((pageRequest.Page - 1) * pageRequest.PageSize)
                .Take(pageRequest.PageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<ShopItem>
            {
                Items = items, TotalCount = totalCount, Page = pageRequest.Page, PageSize = pageRequest.PageSize
            };
        }
    }
}