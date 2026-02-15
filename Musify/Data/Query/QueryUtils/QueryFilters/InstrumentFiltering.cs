using Musify.Dtos.RequestDtos.FilterDtos;
using Musify.Models;
using Musify.Services;

namespace Musify.Data.Query.QueryUtils.QueryFilters
{
    public class InstrumentFiltering : IEntityFiltering<Instrument, InstrumentFilterDto>
    {
        private readonly ICategoryTreeService _categoryTreeService;

        public InstrumentFiltering(ICategoryTreeService categoryTreeService)
        {
            _categoryTreeService = categoryTreeService;
        }
        
        public async Task<IQueryable<Instrument>> Apply(
            IQueryable<Instrument> query,
            InstrumentFilterDto filter,
            CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(filter.Name))
            {
                query = query.Where(i => i.Name.Contains(filter.Name));
            }

            if (!string.IsNullOrWhiteSpace(filter.Brand))
            {
                query = query.Where(i => i.Brand.Contains(filter.Brand));
            }

            if (filter.CategoryId.HasValue)
            {
                var categoryIds = await _categoryTreeService.GetDescendantIdsAsync(
                    (int)filter.CategoryId, // Cast from nullable int
                    cancellationToken);
                
                
                query = query.Where(i => categoryIds.Contains(i.CategoryId));
            }

            return query;
        }
        
    }
}