using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos
{
    public record PageRequest
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int Page { get; init; } = 1;
        
        [Required]
        [Range(10, int.MaxValue)]
        public int PageSize { get; init; } = 10;
    }
}