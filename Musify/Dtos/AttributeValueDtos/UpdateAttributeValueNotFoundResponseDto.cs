using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.AttributeValueDtos
{
    public record UpdateAttributeValueNotFoundResponseDto
    {
        [Required]
        public required string Message { get; set; }
    }
}
