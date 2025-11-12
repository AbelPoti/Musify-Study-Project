using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.AttributeDefinitionDtos
{
    public class AttributeDefinitionUpdateBadRequestResponseDto
    {
        [Required]
        public required string Message { get; set; }
    }
}
