using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.AttributeDefinitionDtos
{
    public record AttributeDefinitionUpdateBadRequestResponseDto
    {
        [Required]
        public required string Message { get; set; }
    }
}
