using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.CategoryDtos
{
    public record CategoryDeleteNotFoundResponseDto
    {
        [Required]
        public required string Message { get; set; }
    }
}
