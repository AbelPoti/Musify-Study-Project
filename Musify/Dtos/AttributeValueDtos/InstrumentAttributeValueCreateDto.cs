namespace Musify.Dtos.AttributeValueDtos
{
    public class InstrumentAttributeValueCreateDto
    {
        public int InstrumentId { get; set; }

        public int AttributeDefinitionId { get; set; }

        public required string Value { get; set; }
    }
}
