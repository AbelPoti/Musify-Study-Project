using Musify.Dtos.RequestDtos.FilterDtos;
using Musify.Models;

namespace Musify.Data.Query.QueryUtils.QueryFilters
{
    public class ShopItemFiltering : IEntityFiltering<ShopItem, ShopItemFilterDto>
    {
        public Task<IQueryable<ShopItem>> Apply(
            IQueryable<ShopItem> query,
            ShopItemFilterDto filter,
            CancellationToken cancellationToken)
        {
            // Repeat instrument filtering logic for now
            if (filter.InstrumentFiter is not null)
            {
                if (!string.IsNullOrWhiteSpace(filter.InstrumentFiter.Name))
                {
                    query = query.Where(sI => sI.Instrument.Name.Contains(filter.InstrumentFiter.Name));
                }

                if (!string.IsNullOrWhiteSpace(filter.InstrumentFiter.Brand))
                {
                    query = query.Where(i => i.Instrument.Brand.Contains(filter.InstrumentFiter.Brand));
                }
            }
            
            // Price filter logic
            if (filter.PriceFilter is not null)
            {
                if (filter.PriceFilter.MinPrice is not null)
                {
                    query = query.Where(sI => sI.Price >= filter.PriceFilter.MinPrice);
                }

                if (filter.PriceFilter.MaxPrice is not null)
                {
                    query = query.Where(sI => sI.Price <= filter.PriceFilter.MaxPrice);
                }
            }

            if (!string.IsNullOrWhiteSpace(filter.Condition))
            {
                query = query.Where(sI => sI.Condition == filter.Condition);
            }

            return Task.FromResult(query);
        }
    }
}