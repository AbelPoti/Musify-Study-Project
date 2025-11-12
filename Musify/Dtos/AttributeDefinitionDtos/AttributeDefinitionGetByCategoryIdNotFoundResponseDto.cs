using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.AttributeDefinitionDtos
{
    public class AttributeDefinitionGetByCategoryIdNotFoundResponseDto
    {
        [Required] public required string Message { get; set; }
    }   
}
