using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Musify.Services
{
    public class EmailSender : IEmailSender
    {
        private WebApplicationBuilder _builder;

        public EmailSender(WebApplicationBuilder builder)
        {
            _builder = builder;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            throw new NotImplementedException();
        }

        public async Task Execute(string apiKey, string subject, string message, string recipient)
        {
            var client = new SendGridClient(apiKey);

            var msg = new SendGridMessage()
            {
                From = new EmailAddress("auth@musify.com", "Musify team"),
                Subject = subject,
                PlainTextContent = message,
                HtmlContent = message
            };

            msg.AddTo(new EmailAddress(recipient));

            // Disable click tracking.
            msg.SetClickTracking(false, false);

            var response = await client.SendEmailAsync(msg);
        }
    }
}
