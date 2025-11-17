using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.AttributeDefinitionDtos
{
    public record AttributeDefinitionUpdateNotFoundResponseDto
    {
        [Required]
        public required string Message { get; set; }
    }
}
