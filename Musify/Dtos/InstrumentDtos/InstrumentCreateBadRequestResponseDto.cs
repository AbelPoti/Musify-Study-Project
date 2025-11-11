using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.InstrumentDtos
{
    public class InstrumentCreateBadRequestResponseDto
    {
        [Required]
        public required string Message { get; set; }
    }
}
