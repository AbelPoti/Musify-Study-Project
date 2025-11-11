using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.AttributeValueDtos
{
    public class AddAttributeValueToInstrumentNotFoundResponseDto
    {
        [Required]
        public required string Message { get; set; }
    }
}
