using Musify.Dtos.RequestDtos.FilterDtos;
using Musify.Models;

namespace Musify.Data.Query.QueryUtils.QueryFilters
{
    public class InstrumentFiltering : IEntityFiltering<Instrument, InstrumentFiterDto>
    {
        public IQueryable<Instrument> Apply(IQueryable<Instrument> query, InstrumentFiterDto filter)
        {
            if (!string.IsNullOrWhiteSpace(filter.Name))
            {
                query = query.Where(i => i.Name.Contains(filter.Name));
            }

            if (!string.IsNullOrWhiteSpace(filter.Brand))
            {
                query = query.Where(i => i.Brand.Contains(filter.Brand));
            }

            return query;
        }
    }
}