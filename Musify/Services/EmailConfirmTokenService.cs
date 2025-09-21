using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace Musify.Services
{
    public class EmailConfirmTokenService : IEmailConfirmTokenService
    {
        private readonly UserManager<IdentityUser> _userManager;

        public EmailConfirmTokenService(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<string> GenerateEmailConfirmationToken(string username)
        {
            // Fetch user again for Id
            var user = await _userManager.FindByNameAsync(username);

            string emailConfirmToken = await _userManager.GenerateEmailConfirmationTokenAsync(user!);
            // Since tokens may contain special characters, encode it
            return WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(emailConfirmToken));
        }
    }
}
