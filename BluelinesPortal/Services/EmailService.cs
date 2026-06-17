using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace BluelinesPortal.Services
{
    // 1. The Interface
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string toName, string subject, string htmlBody);
    }

    // 2. The Implementation
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string toName, string subject, string htmlBody)
        {
            var emailConfig = _config.GetSection("EmailConfiguration");

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(emailConfig["SenderName"], emailConfig["SenderEmail"]));
            message.To.Add(new MailboxAddress(toName, toEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = htmlBody };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            try
            {
                // Connect and authenticate
                await client.ConnectAsync(emailConfig["SmtpServer"], int.Parse(emailConfig["SmtpPort"]), SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(emailConfig["SenderEmail"], emailConfig["Password"]);

                // Send the email
                await client.SendAsync(message);
            }
            finally
            {
                await client.DisconnectAsync(true);
            }
        }
    }
}