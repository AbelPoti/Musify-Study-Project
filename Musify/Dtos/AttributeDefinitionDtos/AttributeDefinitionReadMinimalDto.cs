using Musify.Models;

namespace Musify.Dtos.AttributeDefinitionDtos
{
    public record AttributeDefinitionReadMinimalDto
    {
        public required int Id { get; init; }

        public required string Name { get; init; }

        public required AttributeDefinitionDataType DataType { get; init; }

        public required int CategoryId { get; init; }
    }
}
