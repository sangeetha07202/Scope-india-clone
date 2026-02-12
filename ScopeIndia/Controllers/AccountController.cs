using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mail;
using System;
using ScopeIndia.Data;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _db;

    public AccountController(ApplicationDbContext db)
    {
        _db = db;
    }

    // ----------------- FORGOT PASSWORD -----------------
    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    public IActionResult ForgotPassword(string Email)
    {
        var user = _db.Students.FirstOrDefault(x => x.Email == Email);
        if (user == null)
        {
            TempData["Error"] = "Email not found!";
            return View();
        }

        // Generate OTP
        var otp = new Random().Next(100000, 999999).ToString();

        // Save OTP temporarily
        user.OTP= otp;
        user.OtpExpireTime = DateTime.Now.AddMinutes(10);
        _db.SaveChanges();

        // Send Email
        SendOtpEmail(Email, otp);

        ViewBag.Email = Email;
        return RedirectToAction("VerifyOtp", new { email = Email });
    }

    // ----------------- VERIFY OTP -----------------
    [HttpGet]
    public IActionResult VerifyOtp(string email)
    {
        ViewBag.Email = email;
        return View();
    }

    [HttpPost]
    public IActionResult VerifyOtp(string Email, string Otp)
    {
        var user = _db.Students.FirstOrDefault(x => x.Email == Email);

        if (user == null || user.OTP != Otp || user.OtpExpireTime < DateTime.Now)
        {
            TempData["Error"] = "Invalid or expired OTP!";
            ViewBag.Email = Email;
            return View();
        }

        return RedirectToAction("ResetPassword", new { email = Email });
    }

    // ----------------- RESET PASSWORD -----------------
    [HttpGet]
    public IActionResult ResetPassword(string email)
    {
        ViewBag.Email = email;
        return View();
    }

    
    [HttpPost]
    public IActionResult ResetPassword(string Email, string NewPassword, string ConfirmPassword)
    {
        if (NewPassword != ConfirmPassword)
        {
            TempData["Error"] = "Passwords do not match!";
            ViewBag.Email = Email;
            return View();
        }

        var user = _db.Students.FirstOrDefault(x => x.Email == Email);

        if (user == null)
        {
            TempData["Error"] = "User not found!";
            ViewBag.Email = Email;
            return View();
        }

        // Update password
        user.Password = NewPassword;
        user.OTP = null;
        user.OtpExpireTime = null;
        _db.SaveChanges();

        // Show success alert
        TempData["Success"] = "Password updated successfully!";
        ViewBag.Email = Email;

        return View(); // stay on same page, DO NOT REDIRECT
    }















    // ---------------- EMAIL SENDER ----------------
    private void SendOtpEmail(string email, string otp)
    {
        var msg = new MailMessage();
        msg.From = new MailAddress("gk9085549@gmail.com");
        msg.To.Add(email);
        msg.Subject = "Your Password Reset OTP";
        msg.Body = $"Your OTP is: {otp}";

        var smtp = new SmtpClient("smtp.gmail.com")
        {
            Port = 587,
            Credentials = new NetworkCredential("gk9085549@gmail.com", "mlnm ckhg kkpj mpkb"),
            EnableSsl = true
        };

        smtp.Send(msg);
    }
}