using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos.AuthDtos
{
    public class RegisterDto
    {
        [Required]
        [StringLength(50, MinimumLength = 6, ErrorMessage = "Username must be between 6 and 50 characters long")]
        [RegularExpression(@"^(?=.*[a-z])[a-zA-Z0-9_]+$", ErrorMessage = "Username must contain at least one lowercase letter, and can only contain letters, numbers, and underscores")]
        public required string Username { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(254, ErrorMessage = "Email must be less than 255 characters")]
        public required string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters long")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[a-zA-Z0-9_\-*()]+$", ErrorMessage = "Password must contain at least one lowercase letter, one uppercase letter and a digit, and can only contain letters, numbers, hyphens, underscores, asterisks and parentheses")]
        public required string Password { get; set; }
    }
}
