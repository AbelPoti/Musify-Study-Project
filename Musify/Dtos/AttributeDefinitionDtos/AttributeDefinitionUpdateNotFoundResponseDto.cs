using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.AttributeDefinitionDtos
{
    public class AttributeDefinitionUpdateNotFoundResponseDto
    {
        [Required]
        public required string Message { get; set; }
    }
}
