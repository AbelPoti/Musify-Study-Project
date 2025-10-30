using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.AuthDtos
{
    public class ForgotPasswordDto
    {
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public required string Email { get; set; }
    }
}
