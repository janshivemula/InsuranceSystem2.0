using InsuranceSystem2._0.Services.Interfaces;
using InsuranceSystem2._0.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace InsuranceSystem2._0.Services.Implementations
{
    // Works with Mailtrap, smtp4dev, Gmail, SendGrid... only appsettings values change.
    
    public class SmtpEmailSender : IEmailSender
    {
        private readonly EmailSettings _settings;

        public SmtpEmailSender(IOptions<EmailSettings> options)
        {
            _settings = options.Value;
        }

        public async Task SendAsync(string toEmail, string subject, string htmlBody)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;
            message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

            using var client = new SmtpClient();

            // Auto picks STARTTLS or SSL depending on the port (2525/587/465)
            await client.ConnectAsync(
                _settings.Host,
                _settings.Port,
                _settings.EnableSsl ? SecureSocketOptions.Auto : SecureSocketOptions.None);

            await client.AuthenticateAsync(_settings.Username, _settings.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}