using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScopeIndia.Data;
using ScopeIndia.Models;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ScopeIndia.Controllers
{
    public class LoginController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LoginController(ApplicationDbContext context)
        {
            _context = context;
        }

        
        [HttpGet]
        public IActionResult Index()
        {
            return View("Login");
        }

       
        [HttpPost]
        public async Task<IActionResult> Index(Login model)
        {
            if (!ModelState.IsValid)
                return View("Login", model);

            var user = await _context.Logins.FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null)
            {
                ViewBag.Error = "Invalid email or password.";
                return View("Login", model);
            }

            if (user.PasswordHash != HashPassword(model.PasswordHash))
            {
                ViewBag.Error = "Incorrect password.";
                return View("Login", model);
            }

            // Store session
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetInt32("UserId", user.Id);

            return RedirectToAction("Dashboard", "Student");
        }

        
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(Login model)
        {
            if (string.IsNullOrEmpty(model.Email))
            {
                ViewBag.Error = "Please enter your email.";
                return View("Login", model);
            }

            var user = await _context.Logins.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null)
            {
                ViewBag.Error = "No account found with this email.";
                return View("Login", model);
            }

            model.TempPassword = GenerateTempPassword();
            user.PasswordHash = HashPassword(model.TempPassword);
            await _context.SaveChangesAsync();

            ViewBag.Success = $"Temporary password generated: {model.TempPassword}";
            return View("Login", model);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(Login model)
        {
            if (model.NewPassword != model.ConfirmPassword)
            {
                ViewBag.Error = "Passwords do not match.";
                return View("Login", model);
            }

            var user = await _context.Logins.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null)
            {
                ViewBag.Error = "Email not found.";
                return View("Login", model);
            }

            user.PasswordHash = HashPassword(model.NewPassword);
            user.IsFirstLogin = false;
            await _context.SaveChangesAsync();

            ViewBag.Success = "Password reset successfully! You can now log in.";
            return View("Login", model);
        }

        // ---------------- LOGOUT ----------------
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }

        
        private string HashPassword(string? password)
        {
            if (string.IsNullOrEmpty(password)) return "";
            using (var sha = SHA256.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }

      
        private string GenerateTempPassword()
        {
            return Guid.NewGuid().ToString("N").Substring(0, 8);
        }
    }
}