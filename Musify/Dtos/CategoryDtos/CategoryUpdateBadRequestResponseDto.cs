using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.CategoryDtos
{
    public record CategoryUpdateBadRequestResponseDto
    {
        [Required]
        public required string Message { get; set; }
    }
}
