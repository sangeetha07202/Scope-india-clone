using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace eprogram.Services
{
    public class EmailService
    {
        public async Task SendEmailAsync(string from, string to, string subject, string htmlbody)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("", from));
            message.To.Add(new MailboxAddress("", to));
            message.Subject = subject;

            message.Body = new TextPart("html")
            {
                Text = htmlbody
            };

            using var client = new SmtpClient();
            await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync("gk9085549@gmail.com", "vsmr omof khkz jfgu");
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}