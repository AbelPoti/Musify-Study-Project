using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.AuthDtos
{
    public class ResetPasswordOkResponseDto
    {
        [Required]
        public required string Message { get; set; }
    }
}
