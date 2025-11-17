using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.CategoryDtos
{
    public record CategoryUpdateNotFoundResponseDto
    {
        [Required]
        public required string Message { get; set; }
    }
}
