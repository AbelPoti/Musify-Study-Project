using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos
{
    public record SortRequest
    {
        [MaxLength(50)]
        public string? SortBy { get; init; }

        [Required]
        [Range(0, 1)]
        public bool Descending { get; init; } = false;
    }
}