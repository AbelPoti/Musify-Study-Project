using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.InstrumentDtos
{
    public record InstrumentUpdateBadRequestResponseDto
    {
        [Required]
        public required string Message { get; set; }
    }
}
