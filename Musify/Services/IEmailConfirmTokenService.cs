using Musify.Models;

namespace Musify.Services
{
    public interface IEmailConfirmTokenService
    {
        Task<string> GenerateEmailConfirmationToken(ApplicationUser user);
    }
}
