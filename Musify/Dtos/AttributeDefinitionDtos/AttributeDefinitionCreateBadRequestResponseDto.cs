using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.AttributeDefinitionDtos
{
    public class AttributeDefinitionCreateBadRequestResponseDto
    {
        [Required]
        public required string Message { get; set; }
    }
}
