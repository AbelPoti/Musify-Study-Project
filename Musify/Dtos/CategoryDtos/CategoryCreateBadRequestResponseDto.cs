using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.CategoryDtos
{
    public class CategoryCreateBadRequestResponseDto
    {
        [Required]
        public required string Message { get; set; }
    }
}
