using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.CategoryDtos
{
    public record CategoryCreateBadRequestResponseDto
    {
        [Required]
        public required string Message { get; set; }
    }
}
