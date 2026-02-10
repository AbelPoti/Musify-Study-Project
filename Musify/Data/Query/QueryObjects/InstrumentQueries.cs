using Microsoft.EntityFrameworkCore;
using Musify.Data.DatabaseContext;
using Musify.Data.Query.QueryUtils;
using Musify.Data.Query.QueryUtils.QueryFilters;
using Musify.Dtos.RequestDtos;
using Musify.Dtos.RequestDtos.FilterDtos;
using Musify.Models;

namespace Musify.Data.Query.QueryObjects
{
    public class InstrumentQueries : IQueries<Instrument, InstrumentFiterDto>
    {
        private readonly MusifyDbContext _dbContext;
        
        private readonly InstrumentFiltering _instrumentFiltering;
        
        public InstrumentQueries(MusifyDbContext dbContext,  InstrumentFiltering instrumentFiltering)
        {
            _dbContext = dbContext;
            _instrumentFiltering = instrumentFiltering;
        }
        
        public async Task<PagedResult<Instrument>> GetItemsAsync(
            PageRequest pageRequest,
            SortRequest sortRequest,
            InstrumentFiterDto instrumentFiterDto,
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
            
            // Apply filtering
            query = _instrumentFiltering.Apply(query, instrumentFiterDto);
            
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