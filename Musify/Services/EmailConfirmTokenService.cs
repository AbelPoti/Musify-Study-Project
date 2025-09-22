using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Musify.Models;
using System.Text;

namespace Musify.Services
{
    public class EmailConfirmTokenService : IEmailConfirmTokenService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public EmailConfirmTokenService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<string> GenerateEmailConfirmationToken(ApplicationUser user)
        {
            string emailConfirmToken = await _userManager.GenerateEmailConfirmationTokenAsync(user!);
            // Since tokens may contain special characters, encode it
            return WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(emailConfirmToken));
        }
    }
}
