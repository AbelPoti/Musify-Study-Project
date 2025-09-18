using Microsoft.AspNetCore.Identity.UI.Services;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Musify.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var client = new SendGridClient(_configuration["EmailService:ApiKey"]);

            var msg = new SendGridMessage()
            {
                From = new EmailAddress("noreply@em7171.api.learningaspproject.org", "Musify team"),
                Subject = subject,
                HtmlContent = htmlMessage
            };

            msg.AddTo(new EmailAddress(email));

            // Disable click tracking.
            msg.SetClickTracking(false, false);

            await client.SendEmailAsync(msg);
        }
    }
}
