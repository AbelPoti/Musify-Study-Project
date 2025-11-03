using Microsoft.AspNetCore.WebUtilities;

namespace Musify.Tests
{
    internal static class TestUtils
    {
        public static string GenerateTestToken(int length = 64)
        {
            var randomBytes = new byte[length];
            new Random().NextBytes(randomBytes);
            return WebEncoders.Base64UrlEncode(randomBytes);
        }
    }
}
