namespace Musify.Dtos.AttributeValueDtos
{
    public class InstrumentAttributeValueReadMinimalDto
    {
        public int Id { get; set; }

        public int InstrumentId { get; set; }

        public int AttributeDefinitionId { get; set; }

        public required string Value { get; set; }
    }
}
