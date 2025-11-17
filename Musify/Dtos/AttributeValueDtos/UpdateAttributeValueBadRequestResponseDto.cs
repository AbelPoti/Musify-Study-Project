using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.AttributeValueDtos
{
    public record UpdateAttributeValueBadRequestResponseDto
    {
        [Required]
        public required string Message { get; set; }
    }
}
