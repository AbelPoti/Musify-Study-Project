using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.RequestDtos.FilterDtos
{
    public record InstrumentFiterDto
    {
        [MaxLength(256)]
        public string? Name { get; init; }
        
        [MaxLength(128)]
        public string? Brand { get; init; }
        
        // TODO Create filtering based on categories
    }
}