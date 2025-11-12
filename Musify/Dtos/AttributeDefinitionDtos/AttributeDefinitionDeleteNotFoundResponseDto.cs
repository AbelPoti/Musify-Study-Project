using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.AttributeDefinitionDtos
{
    public class AttributeDefinitionDeleteNotFoundResponseDto
    {
        [Required]
        public required string Message { get; set; }
    }
}
