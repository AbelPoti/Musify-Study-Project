using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.AttributeValueDtos
{
    public class DeleteAttributeValueNotFoundResponseDto
    {
        [Required]
        public required string Message { get; set; }
    }
}
