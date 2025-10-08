using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.Auth
{
    public class ForgotPasswordDto
    {
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public required string Email { get; set; }
    }
}
