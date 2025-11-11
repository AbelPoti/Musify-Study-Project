using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.InstrumentDtos
{
    public class InstrumentUpdateNotFoundResponseDto
    {
        [Required]
        public required string Message { get; set; }
    }
}
