using Microsoft.EntityFrameworkCore;
using Musify.Data.DatabaseContext;
using Musify.Data.Query.QueryUtils;
using Musify.Dtos.RequestDtos;
using Musify.Models;

namespace Musify.Data.Query.QueryObjects
{
    public class InstrumentQueries : IQueries<Instrument>
    {
        private readonly MusifyDbContext _dbContext;
        
        public InstrumentQueries(MusifyDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        
        public async Task<PagedResult<Instrument>> GetItemsAsync(
            PageRequest pageRequest,
            SortRequest sortRequest,
            CancellationToken cancellationToken)
        {
            IQueryable<Instrument> query = _dbContext.Instruments.AsNoTracking();
            
            // Sorting - explicit mapping with fallback
            query = sortRequest.SortBy switch
            {
                "name" => sortRequest.Descending
                    ? query.OrderByDescending(sI => sI.Name)
                    :  query.OrderBy(sI => sI.Name),
                
                "brand" => sortRequest.Descending
                    ? query.OrderByDescending(sI => sI.Brand)
                    : query.OrderBy(sI => sI.Brand),
                
                _ => query.OrderBy(i => i.Id)
            };
            
            int totalCount = await query.CountAsync(cancellationToken);
            
            List<Instrument> items = await query
                .Skip((pageRequest.Page - 1) * pageRequest.PageSize)
                .Take(pageRequest.PageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<Instrument>
            {
                Items = items, TotalCount = totalCount, Page = pageRequest.Page, PageSize = pageRequest.PageSize
            };
        }
    }
}