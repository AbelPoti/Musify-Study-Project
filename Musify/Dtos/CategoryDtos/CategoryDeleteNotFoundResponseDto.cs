using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.CategoryDtos
{
    public class CategoryDeleteNotFoundResponseDto
    {
        [Required]
        public required string Message { get; set; }
    }
}
