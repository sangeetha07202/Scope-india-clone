using eprogram.Models;
using eprogram.Services;
using Microsoft.AspNetCore.Mvc;

namespace eprogram.Controllers
{
    public class HomeController : Controller
    {
        private readonly EmailService _emailService;

        public HomeController(EmailService emailService)
        {
            _emailService = emailService;
        }

        // Load form
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // Handle form post
        [HttpPost]
        public async Task<IActionResult> Send(EmailViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            await _emailService.SendEmailAsync(model.From, model.To, model.Subject, model.Message);

            ViewBag.Msg = "Email Sent Successfully!";
            return View("Index");
        }
    }
}