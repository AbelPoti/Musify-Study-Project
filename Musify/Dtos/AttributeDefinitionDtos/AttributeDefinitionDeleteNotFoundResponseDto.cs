using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.AttributeDefinitionDtos
{
    public record AttributeDefinitionDeleteNotFoundResponseDto
    {
        [Required]
        public required string Message { get; set; }
    }
}
