using Microsoft.AspNetCore.Identity;

namespace Musify.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        public DateTimeOffset? RegistrationTime { get; set; }

        public DateTimeOffset? LastLoginTime { get; set; }

        public DateTimeOffset? LastConfirmEmailSent { get; set; }

        public DateTimeOffset? LastPasswordResetSent { get; set; }
    }
}
