using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.AttributeValueDtos
{
    public record AddAttributeValueToInstrumentBadRequestResponseDto
    {
        [Required]
        public required string Message { get; set; }
    }
}
