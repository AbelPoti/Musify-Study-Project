using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.RequestDtos.FilterDtos
{
    public record PriceFilterDto
    {
        [Range((double)decimal.Zero, (double)decimal.MaxValue)]
        public decimal? MinPrice { get; init; }
        
        [Range((double)decimal.Zero, (double)decimal.MaxValue)]
        public decimal? MaxPrice { get; init; }
    }
}