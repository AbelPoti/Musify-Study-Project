using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.AuthDtos
{
    public class ForgotPasswordOkResponseDto
    {
        [Required]
        public required string Message { get; set; }
    }
}
