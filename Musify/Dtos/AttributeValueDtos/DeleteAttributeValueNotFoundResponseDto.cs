using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.AttributeValueDtos
{
    public record DeleteAttributeValueNotFoundResponseDto
    {
        [Required]
        public required string Message { get; set; }
    }
}
