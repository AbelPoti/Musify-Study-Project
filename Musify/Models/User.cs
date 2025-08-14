using System.ComponentModel.DataAnnotations;

namespace Musify.Models
{
    public class User : ModelBase
    {
        [Required]
        public required string Username { get; set; }

        [Required]
        public required string PasswordHash { get; set; }

        [Required]
        public required string Email { get; set; }

        public required DateTime CreatedAt { get; set; }

        public DateTime? LastLogin { get; set; }

        public User() { }

        public User(string username, string passwordHash, string email)
        {
            Username = username;
            PasswordHash = passwordHash;
            Email = email;
            CreatedAt = DateTime.UtcNow;
        }
    }
}
