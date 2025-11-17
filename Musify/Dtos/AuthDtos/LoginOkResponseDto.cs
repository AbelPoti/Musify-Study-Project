using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.AuthDtos
{
    public record LoginOkResponseDto
    {
        [Required]
        public required string Message { get; set; }

        [Required]
        public required string JwtToken { get; set; }
    }
}
