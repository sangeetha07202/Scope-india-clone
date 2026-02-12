using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace ScopeIndia.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
        {
            // ✅ Read SMTP settings from appsettings.json
            var smtpSection = _config.GetSection("Smtp");
            string host = smtpSection["Host"] ?? throw new Exception("SMTP Host not configured");
            int port = smtpSection.GetValue<int>("Port");
            string username = smtpSection["Username"] ?? throw new Exception("SMTP Username not configured");
            string password = smtpSection["Password"] ?? throw new Exception("SMTP Password not configured");
            string from = smtpSection["From"] ?? username;
            string fromName = smtpSection["FromName"] ?? "SCOPE India";

            using (var mail = new MailMessage())
            {
                mail.From = new MailAddress(from, fromName);
                mail.To.Add(toEmail);
                mail.Subject = subject;
                mail.Body = htmlMessage;
                mail.IsBodyHtml = true;

                using (var smtp = new SmtpClient(host, port))
                {
                    smtp.Credentials = new NetworkCredential(username, password);
                    smtp.EnableSsl = true;
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;

                    await smtp.SendMailAsync(mail);
                }
            }
        }
    }
}