using MailKit.Security;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using ScopeIndia.Models;
using Microsoft.Extensions.Configuration;

namespace ScopeIndia.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _config;

       
        public HomeController(IConfiguration config)
        {
            _config = config;
        }

        public IActionResult Index() => View();

       
            public IActionResult About()
            {
                return View();
            }
        
        [HttpGet]
        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Contact(Contact model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                
                var email = new MimeMessage();
                email.From.Add(new MailboxAddress("Scope India Website", _config["Smtp:From"]));

               
                email.To.Add(MailboxAddress.Parse("rithuvava77@gmail.com"));

                
                if (!string.IsNullOrEmpty(model.Email))
                {
                    email.ReplyTo.Add(new MailboxAddress(model.Name ?? "Visitor", model.Email));
                }

                email.Subject = model.Subject ?? "New Contact Form Message";

                var builder = new BodyBuilder
                {
                    HtmlBody = $@"
                        <h3>📩 New Contact Message</h3>
                        <p><strong>Name:</strong> {model.Name}</p>
                        <p><strong>Email:</strong> {model.Email}</p>
                        <p><strong>Subject:</strong> {model.Subject}</p>
                        <p><strong>Message:</strong></p>
                        <p>{model.Message}</p>"
                };

                email.Body = builder.ToMessageBody();

                
                using var smtp = new MailKit.Net.Smtp.SmtpClient();
                var smtpHost = _config["Smtp:Host"];
                var smtpPort = int.TryParse(_config["Smtp:Port"], out int port) ? port : 587;
                var smtpUser = _config["Smtp:Username"];
                var smtpPass = _config["Smtp:Password"];

                await smtp.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(smtpUser, smtpPass);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);

                
                ViewBag.Success = "✅ Your message has been sent successfully!";
                ModelState.Clear();
            }
            catch (Exception ex)
            {
                
                ViewBag.Error = $"❌ Email sending failed: {ex.Message}";
            }

            return View();
        }
    }
}