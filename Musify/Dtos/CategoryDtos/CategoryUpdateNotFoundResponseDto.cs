using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.CategoryDtos
{
    public class CategoryUpdateNotFoundResponseDto
    {
        [Required]
        public required string Message { get; set; }
    }
}
