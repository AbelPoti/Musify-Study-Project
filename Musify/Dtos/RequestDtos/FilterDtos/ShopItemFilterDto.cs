using Musify.Models;

namespace Musify.Dtos.RequestDtos.FilterDtos
{
    public record ShopItemFilterDto
    {
        public InstrumentFiterDto? InstrumentFiter { get; init; }
        
        public PriceFilterDto? PriceFilter { get; init; }
        
        public string? Condition { get; init; }
    }
}