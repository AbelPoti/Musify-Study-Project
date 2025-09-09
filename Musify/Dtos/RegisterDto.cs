using System.ComponentModel.DataAnnotations;

namespace Musify.Dtos
{
    public class RegisterDto
    {
        [Required]
        public required string Username { get; set; }

        [Required]
        public required string Email { get; set; }

        [Required]
        public required string Password { get; set; }
    }
}
