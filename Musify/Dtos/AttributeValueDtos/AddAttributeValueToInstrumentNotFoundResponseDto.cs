using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.AttributeValueDtos
{
    public record AddAttributeValueToInstrumentNotFoundResponseDto
    {
        [Required]
        public required string Message { get; set; }
    }
}
