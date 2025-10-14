namespace Musify.Dtos.AttributeValueDtos
{
    public class AttributeValueCreateDto
    {
        public int InstrumentId { get; set; }

        public int AttributeDefinitionId { get; set; }

        public required string Value { get; set; }
    }
}
