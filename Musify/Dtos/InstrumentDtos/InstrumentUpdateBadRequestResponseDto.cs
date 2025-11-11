using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.InstrumentDtos
{
    public class InstrumentUpdateBadRequestResponseDto
    {
        [Required]
        public required string Message { get; set; }
    }
}
