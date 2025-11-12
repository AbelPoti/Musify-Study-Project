using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.AttributeValueDtos
{
    public class UpdateAttributeValueBadRequestResponseDto
    {
        [Required]
        public required string Message { get; set; }
    }
}
