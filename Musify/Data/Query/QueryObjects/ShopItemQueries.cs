using Microsoft.EntityFrameworkCore;
using Musify.Data.DatabaseContext;
using Musify.Data.Query.QueryUtils;
using Musify.Data.Query.QueryUtils.QueryFilters;
using Musify.Dtos.RequestDtos;
using Musify.Dtos.RequestDtos.FilterDtos;
using Musify.Models;

namespace Musify.Data.Query.QueryObjects
{
    public class ShopItemQueries : IQueries<ShopItem, ShopItemFilterDto>
    {
        private readonly MusifyDbContext _dbContext;
        
        private readonly IEntityFiltering<ShopItem, ShopItemFilterDto> _shopItemFiltering;
        
        public ShopItemQueries(
            MusifyDbContext dbContext,
            IEntityFiltering<ShopItem, ShopItemFilterDto> shopItemFiltering)
        {
            _dbContext = dbContext;
            _shopItemFiltering = shopItemFiltering;
        }
        
        public async Task<PagedResult<ShopItem>> GetItemsAsync(
            PageRequest pageRequest,
            SortRequest? sortRequest,
            ShopItemFilterDto? filter,
            CancellationToken cancellationToken)
        {
            IQueryable<ShopItem> query = _dbContext.ShopItems.AsNoTracking();
            
            // Sorting - explicit mapping with fallback
            if (sortRequest != null)
            {
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
            }

            if (filter != null)
            {
                query = _shopItemFiltering.Apply(query, filter);
            }
            
            int totalCount = await query.CountAsync(cancellationToken);
            
            List<ShopItem> items = await query
                .Skip((pageRequest.Page - 1) * pageRequest.PageSize)
                .Take(pageRequest.PageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<ShopItem>(items, totalCount, pageRequest.Page, pageRequest.PageSize);
        }
    }
}