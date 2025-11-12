using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.AttributeValueDtos
{
    public class UpdateAttributeValueNotFoundResponseDto
    {
        [Required]
        public required string Message { get; set; }
    }
}
