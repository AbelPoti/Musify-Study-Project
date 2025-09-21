using Microsoft.AspNetCore.Identity;

namespace Musify.Services
{
    public interface IEmailConfirmTokenService
    {
        Task<string> GenerateEmailConfirmationToken(IdentityUser user);
    }
}
