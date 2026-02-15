using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.RequestDtos
{
    public record PageRequest
    {
        [Range(1, int.MaxValue)]
        public int Page { get; init; }
        
        [Range(1, 20)]
        public int PageSize { get; init; }
    }
}