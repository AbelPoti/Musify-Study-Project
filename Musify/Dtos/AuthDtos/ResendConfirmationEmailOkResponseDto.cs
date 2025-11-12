using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.AuthDtos
{
    public class ResendConfirmationEmailOkResponseDto
    {
        [Required]
        public required string Message { get; set; }
    }
}
