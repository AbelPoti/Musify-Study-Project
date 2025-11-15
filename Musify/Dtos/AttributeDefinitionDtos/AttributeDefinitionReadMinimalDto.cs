using Musify.Models;

namespace Musify.Dtos.AttributeDefinitionDtos
{
    public record AttributeDefinitionReadMinimalDto(
        int Id,
        string Name,
        AttributeDefinitionDataType DataType,
        int CategoryId);
}
