using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.InstrumentDtos
{
    public record InstrumentUpdateNotFoundResponseDto
    {
        [Required]
        public required string Message { get; set; }
    }
}
