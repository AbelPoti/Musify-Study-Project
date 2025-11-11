using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.AttributeValueDtos
{
    public class DeleteAttributeValueBadRequestResponseDto
    {
        [Required]
        public required string Message { get; set; }
    }
}
