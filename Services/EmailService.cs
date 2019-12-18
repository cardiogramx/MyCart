using System.Threading;
using System.Threading.Tasks;
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
        public async Task<bool> SendAsync(SendGridEmailMessage message, CancellationToken cancellationToken = default)
        {

            var apiKey = "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";

            var client = new SendGridClient(apiKey);

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