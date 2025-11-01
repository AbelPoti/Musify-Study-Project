using Musify.Models;

namespace Musify.Services
{
    public interface IEmailConfirmTokenService
    {
        Task<string> GenerateEmailConfirmationTokenAsync(ApplicationUser user);
    }
}
