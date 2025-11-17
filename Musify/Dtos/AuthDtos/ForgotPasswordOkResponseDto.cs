using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.AuthDtos
{
    public record ForgotPasswordOkResponseDto
    {
        [Required]
        public required string Message { get; set; }
    }
}
