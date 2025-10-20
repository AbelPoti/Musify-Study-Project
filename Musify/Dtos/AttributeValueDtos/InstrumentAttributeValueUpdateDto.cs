namespace Musify.Dtos.AttributeValueDtos
{
    public class InstrumentAttributeValueUpdateDto
    {
        public int Id { get; set; }

        public int InstrumentId { get; set; }

        public int AttributeDefinitionId { get; set; }

        public required string Value { get; set; }
    }
}
