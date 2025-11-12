using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.CategoryDtos
{
    public class CategoryUpdateBadRequestResponseDto
    {
        [Required]
        public required string Message { get; set; }
    }
}
