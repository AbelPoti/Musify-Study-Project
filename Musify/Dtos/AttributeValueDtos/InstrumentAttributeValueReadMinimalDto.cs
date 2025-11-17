namespace Musify.Dtos.AttributeValueDtos
{
    public record InstrumentAttributeValueReadMinimalDto
    {
        public required int Id { get; init; }

        public required int InstrumentId { get; init; }

        public required int AttributeDefinitionId { get; init; }

        public required string Value { get; init; }
    }
}
