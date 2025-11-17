using Musify.Models;

namespace Musify.Dtos.AttributeDefinitionDtos
{
    public record AttributeDefinitionReadDetailedDto
    {
        public int Id { get; init; }

        public required string Name { get; init; }

        public required AttributeDefinitionDataType DataType { get; init; }

        public int CategoryId { get; init; }

        public required Category Category { get; init; }
    }
}
