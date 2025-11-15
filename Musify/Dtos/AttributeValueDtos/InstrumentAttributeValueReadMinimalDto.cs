namespace Musify.Dtos.AttributeValueDtos
{
    public record InstrumentAttributeValueReadMinimalDto(
        int Id,
        int InstrumentId,
        int AttributeDefinitionId,
        string Value);
}
