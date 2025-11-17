using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.AttributeValueDtos
{
    public record DeleteAttributeValueBadRequestResponseDto
    {
        [Required]
        public required string Message { get; set; }
    }
}
