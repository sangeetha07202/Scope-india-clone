using eprogram.Services;
using Microsoft.AspNetCore.Mvc;

namespace eprogram.Controllers
{
    public class EmailController : Controller
    {
        private readonly EmailService _email;

        public EmailController(EmailService email)
        {
            _email = email;
        }

        [HttpPost]
        public async Task<IActionResult> Send(string from, string to, string subject, string message)
        {
            await _email.SendEmailAsync(from, to, subject, message);

            ViewBag.msg = "Email sent successfully!";
            return View();
        }
    }
}