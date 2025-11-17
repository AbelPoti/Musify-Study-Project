using Musify.Dtos.AttributeDefinitionDtos;

namespace Musify.Dtos.AttributeValueDtos
{
    public record InstrumentAttributeValueReadDetailedDto
    {
        public int Id { get; init; }

        public int InstrumentId { get; init; }

        public int AttributeDefinitionId { get; init; }

        public required AttributeDefinitionReadMinimalDto AttributeDefinition { get; init; }

        public required string Value { get; init; }
    }
}
