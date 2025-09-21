namespace Musify.Services
{
    public interface IEmailConfirmTokenService
    {
        Task<string> GenerateEmailConfirmationToken(string username);
    }
}
