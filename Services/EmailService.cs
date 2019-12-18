using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace MyCart.Services
{
    public interface IEmailService
    {
        Task<bool> SendAsync(SendGridEmailMessage message, CancellationToken cancellationToken = default);
    }


    public class EmailService : IEmailService
    {
        private readonly string sendGridApiKey;

        public EmailService(IConfiguration configuration)
        {
            this.sendGridApiKey = configuration.GetSection("SendGrid:ApiKey").Get<string>();
        }

        public async Task<bool> SendAsync(SendGridEmailMessage message, CancellationToken cancellationToken = default)
        {
            var client = new SendGridClient(sendGridApiKey);

            var from = new EmailAddress("cardiogramx@gmail.com", "Cardiogramx from MyCart");
            var to = new EmailAddress(message.Address);

            var msg = MailHelper
                .CreateSingleEmail(from, to, message.Subject, message.PlainTextContent, message.HtmlContent);

            var response = await client.SendEmailAsync(msg, cancellationToken);

            if (response.StatusCode != System.Net.HttpStatusCode.Accepted)
            {
                return false;
            }

            return true;
        }
    }

    public class SendGridEmailMessage
    {
        public string Address { get; set; }

        public string Subject { get; set; }

        public string PlainTextContent { get; set; }

        public string HtmlContent { get; set; }
    }
}