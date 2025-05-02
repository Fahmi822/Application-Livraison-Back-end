using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;

namespace Application_Livraison_Backend.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_config["EmailSettings:FromEmail"]));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;

            var builder = new BodyBuilder
            {
                HtmlBody = body
            };
            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();

            // Correction ici :
            string? smtpPortString = _config["EmailSettings:SmtpPort"];
            int smtpPort = 587; // Valeur par défaut, 587 est souvent utilisé pour StartTls
            if (!string.IsNullOrEmpty(smtpPortString))
            {
                smtpPort = int.Parse(smtpPortString);
            }

            await smtp.ConnectAsync(_config["EmailSettings:SmtpServer"], smtpPort, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_config["EmailSettings:SmtpUser"], _config["EmailSettings:SmtpPass"]);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }

    }
}
