using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.InstrumentDtos
{
    public record InstrumentCreateBadRequestResponseDto
    {
        [Required]
        public required string Message { get; set; }
    }
}
