using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace PractoBackend.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var smtpServer = _configuration["EmailSettings:SmtpServer"] ?? "smtp.gmail.com";
            var portString = _configuration["EmailSettings:Port"] ?? "587";
            var senderName = _configuration["EmailSettings:SenderName"] ?? "Practo Backend Team";
            var senderEmail = _configuration["EmailSettings:SenderEmail"];
            var senderPassword = _configuration["EmailSettings:SenderPassword"];

            if (!int.TryParse(portString, out int port))
            {
                port = 587;
            }

            var message = new MailMessage
            {
                From = new MailAddress(senderEmail, senderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            message.To.Add(toEmail);

            using var client = new SmtpClient(smtpServer, port)
            {
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(senderEmail, senderPassword),
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network
            };

            Console.WriteLine($"[EMAIL SERVICE] Attempting to send email to {toEmail} using {smtpServer}:{port} as {senderEmail}");
            try
            {
                await client.SendMailAsync(message);
                Console.WriteLine($"[EMAIL SERVICE] Successfully sent email to {toEmail}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EMAIL SERVICE] FAILED to send email to {toEmail}: {ex.Message}");
                throw;
            }
        }
    }
}
