using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos
{
    public class ResetPasswordDto
    {
        [Required]
        public required string UserId { get; set; }

        [Required]
        public required string Token { get; set; }

        [Required]
        public required string NewPassword { get; set; }
    }
}
