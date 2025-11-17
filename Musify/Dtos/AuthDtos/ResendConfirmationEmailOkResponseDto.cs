using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.AuthDtos
{
    public record ResendConfirmationEmailOkResponseDto
    {
        [Required]
        public required string Message { get; set; }
    }
}
