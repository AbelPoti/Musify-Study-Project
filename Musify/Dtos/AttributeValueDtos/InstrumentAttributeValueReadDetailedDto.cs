using Musify.Dtos.AttributeDefinitionDtos;

namespace Musify.Dtos.AttributeValueDtos
{
    public record InstrumentAttributeValueReadDetailedDto
    {
        public int Id { get; set; }

        public int InstrumentId { get; set; }

        public int AttributeDefinitionId { get; set; }

        public required AttributeDefinitionReadMinimalDto AttributeDefinition { get; set; }

        public required string Value { get; set; }
    }
}
