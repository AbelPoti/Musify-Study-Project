using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos
{
    public class RegisterDto
    {
        [Required]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters long")]
        [RegularExpression("^[a-zA-Z0-9_-]+$", ErrorMessage = "Username can only contain letters, numbers, hyphens and underscores")]
        public required string Username { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(254, ErrorMessage = "Email must be less than 255 characters")]
        public required string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters long")]
        public required string Password { get; set; }
    }
}
