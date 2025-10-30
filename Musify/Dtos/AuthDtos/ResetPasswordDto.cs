using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.AuthDtos
{
    public class ResetPasswordDto
    {
        [Required]
        public required string UserId { get; set; }

        [Required]
        public required string Token { get; set; }

        [Required]
        public required string NewPassword { get; set; }

        [Required]
        public required string ConfirmPassword { get; set; }
    }
}
