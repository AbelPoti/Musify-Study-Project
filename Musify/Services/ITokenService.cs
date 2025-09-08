using Microsoft.AspNetCore.Identity;

namespace Musify.Services
{
    public interface ITokenService
    {
        string GenerateToken(IdentityUser user, IList<string> roles);
    }
}
