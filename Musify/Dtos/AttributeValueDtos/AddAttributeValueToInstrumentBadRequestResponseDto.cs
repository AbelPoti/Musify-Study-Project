using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.AttributeValueDtos
{
    public class AddAttributeValueToInstrumentBadRequestResponseDto
    {
        [Required]
        public required string Message { get; set; }
    }
}
