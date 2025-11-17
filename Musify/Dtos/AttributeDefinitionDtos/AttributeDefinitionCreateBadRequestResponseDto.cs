using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.AttributeDefinitionDtos
{
    public record AttributeDefinitionCreateBadRequestResponseDto
    {
        [Required]
        public required string Message { get; set; }
    }
}
