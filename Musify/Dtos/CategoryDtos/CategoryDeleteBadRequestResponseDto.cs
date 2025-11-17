using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.CategoryDtos
{
    public record CategoryDeleteBadRequestResponseDto
    {
        [Required]
        public required string Message { get; set; }

        [Required]
        public required IEnumerable<int> ChildCategoryIds { get; set; }
    }
}
