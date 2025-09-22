using Microsoft.AspNetCore.Identity;
using Musify.Models;

namespace Musify.Services
{
    public interface ITokenService
    {
        string GenerateToken(ApplicationUser user, IList<string> roles);
    }
}
