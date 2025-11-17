using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.AuthDtos
{
    public record LoginUnauthorizedResponseDto
    {
        [Required]
        public required string Message { get; set; }
    }
}
