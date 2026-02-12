using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScopeIndia.Data;
using ScopeIndia.Models;
using ScopeIndia.Services;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ScopeIndia.Controllers
{
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IEmailService _emailService;

        public StudentController(ApplicationDbContext context, IWebHostEnvironment env, IEmailService emailService)
        {
            _context = context;
            _env = env;
            _emailService = emailService;
        }


        [HttpGet]
        // ✅ GET: Register
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(Student student, IFormFile? Avatar, string[] Hobbies)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ErrorMessage = "Please fill all required fields correctly.";
                return View(student);
            }

            try
            {
                // ✅ Handle Avatar upload (if provided)
                if (Avatar != null && Avatar.Length > 0)
                {
                    string uploadDir = Path.Combine(_env.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadDir))
                        Directory.CreateDirectory(uploadDir);

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(Avatar.FileName);
                    string filePath = Path.Combine(uploadDir, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await Avatar.CopyToAsync(stream);
                    }

                    student.AvatarPath = "/uploads/" + fileName;
                }

                // ✅ Combine hobbies (array → comma-separated string)
                if (Hobbies != null && Hobbies.Length > 0)
                    student.Hobbies = string.Join(",", Hobbies);

                // ✅ Generate OTP (temporary password)
                string otp = new Random().Next(100000, 999999).ToString();
                student.OTP = otp;
                student.Password = otp;

                student.Username = student.Email?.Split('@')[0] ?? student.FirstName + new Random().Next(100, 999);


                // ✅ Save student to the database
                _context.Students.Add(student);
                await _context.SaveChangesAsync();

                // ✅ Send OTP email
                string subject = "Your SCOPE India Registration - Temporary Password";
                string body = $@"
            <h3>Hello {student.FirstName},</h3>
            <p>Thank you for registering at <b>SCOPE India</b>.</p>
            <p>Your temporary password (OTP) is:</p>
            <h2 style='color:#002b64;'>{otp}</h2>
            <p>Use this password to log in and access your dashboard.</p>
            <br>
            <p>Best regards,<br><b>SCOPE India Team</b></p>";

                await _emailService.SendEmailAsync(student.Email!, subject, body);

                // ✅ Store success info and redirect to Login page
                TempData["Success"] = "Registration successful! Your OTP has been sent to your email.";
                TempData["Email"] = student.Email;

                return RedirectToAction("Login", "Student");
            }
            catch (Exception ex)
            {
                // ✅ Get full inner exception details (real cause)
                var inner = ex.InnerException?.Message ?? ex.Message;

                // ✅ Log to Visual Studio console for debugging
                Console.WriteLine("❌ Registration Error: " + ex.ToString());

                // ✅ Show clean error on the screen
                ViewBag.ErrorMessage = $"Registration failed: {inner}";
                return View(student);
            }
        }
        [HttpGet]
        public IActionResult Login()
        {
            // Read cookies
            string? savedEmail = Request.Cookies["Email"];
            string? savedPassword = Request.Cookies["Password"];

            if (!string.IsNullOrEmpty(savedEmail) && !string.IsNullOrEmpty(savedPassword))
            {
                ViewBag.RememberEmail = savedEmail;
                ViewBag.Password = savedPassword;
                ViewBag.IsRemembered = true;
            }
            else
            {
                ViewBag.RememberEmail = "";
                ViewBag.Password = "";
                ViewBag.IsRemembered = false;
            }

            return View();
        }

        // ✅ POST: Login

        [HttpPost]
        public IActionResult Login(string Email, string Password, bool RememberMe)
        {
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                ViewBag.ErrorMessage = "Please enter both email and password.";
                return View();
            }

            // Allow login using OTP OR Password
            var student = _context.Students.FirstOrDefault(s =>
                s.Email == Email &&
                (s.OTP == Password || s.Password == Password)
            );

            if (student != null)
            {
                // Save session
                HttpContext.Session.SetString("Email", student.Email!);
                HttpContext.Session.SetInt32("StudentId", student.StudentId);
                HttpContext.Session.SetString("StudentName", $"{student.FirstName} {student.LastName}");

                // ✅ Secure Remember Me (Email only)
                if (RememberMe)
                {
                    CookieOptions options = new CookieOptions
                    {
                        Expires = DateTime.Now.AddDays(7),
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict
                    };

                    Response.Cookies.Append("RememberEmail", Email, options);
                }
                else
                {
                    Response.Cookies.Delete("RememberEmail");
                }

                // If coming from change password
                var fromChange = TempData["FromChangePassword"] as string;
                if (fromChange == "true")
                {
                    TempData.Remove("FromChangePassword");
                    return RedirectToAction("Dashboard", "Student");
                }

                // OTP login → Welcome page
                if (student.OTP == Password)
                {
                    TempData["Email"] = student.Email;
                    return RedirectToAction("welcome", "Student");
                }

                // Normal login → Dashboard
                return RedirectToAction("Dashboard", "Student");
            }

            ViewBag.ErrorMessage = "Invalid email or password.";
            return View();
        }



        [HttpPost]
        public IActionResult ChangePassword(string Email, string NewPassword, string ConfirmPassword)
        {
            if (string.IsNullOrEmpty(Email))
            {
                TempData["Error"] = "Email is missing.";
                return RedirectToAction("Welcome");
            }

            if (NewPassword != ConfirmPassword)
            {
                TempData["Error"] = "Passwords do not match!";
                TempData["Email"] = Email;
                return RedirectToAction("Welcome");
            }

            var student = _context.Students.FirstOrDefault(s => s.Email == Email);
            if (student == null)
            {
                TempData["Error"] = "Student not found!";
                return RedirectToAction("Welcome");
            }

            // ✅ Save NEW PASSWORD into Password column
            student.Password = NewPassword;

            // ❌ Do NOT keep OTP after new password is set
            student.OTP = null;

            // Optional: You can also mark that temp password is no longer valid
            student.TempPassword = null;

            _context.SaveChanges();

            // Auto-fill login page
            TempData["Email"] = student.Email;
            TempData["Password"] = NewPassword;

            // Used in Login() to redirect directly to Dashboard
            TempData["FromChangePassword"] = "true";

            TempData["Success"] = "Password changed successfully! Please log in.";

            return RedirectToAction("Login", "Student");
        }

        // ✅ Welcome Page (GET)


        [HttpGet]
        public IActionResult Welcome()
        {
            // Try to get email from TempData or Cookies
            string? email = TempData["Email"] as string ?? Request.Cookies["Email"];

            if (string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "Session expired. Please log in again.";
                return RedirectToAction("Login", "Student");
            }

            var student = _context.Students.FirstOrDefault(s => s.Email == email);
            if (student == null)
            {
                TempData["Error"] = "Student not found.";
                return RedirectToAction("Login", "Student");
            }

            // ✅ Always pass the student model to the view
            return View(student);
        }



        // ✅ STUDENT DASHBOARD

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            string? email = HttpContext.Session.GetString("Email");

            if (string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "Session expired! Please login again.";
                return RedirectToAction("Login");
            }

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.Email == email);

            if (student == null)
            {
                TempData["Error"] = "Student not found!";
                return RedirectToAction("Login");
            }

            int id = student.StudentId;

            // ⭐ FIX: return CourseId + CourseName
            var myCourses = await _context.Enrollments
                .Where(e => e.StudentId == id)
                .Include(e => e.Course)
                .Select(e => new
                {
                    CourseId = e.CourseId,
                    CourseName = e.Course.CourseName
                })
                .ToListAsync();

            ViewBag.MyCourses = myCourses;

            return View(student);
        }


        [HttpPost]
        public async Task<IActionResult> Dashboard(Student model)
        {
            // You usually won’t update anything here
            // but this prevents errors if form accidentally posts

            return RedirectToAction("Dashboard");
        }


        // POST: Student/DeleteEnrollment
        [HttpPost]
    public async Task<IActionResult> DeleteEnrollment(int courseId)
    {
        int? studentId = HttpContext.Session.GetInt32("StudentId");
        if (studentId == null)
        {
            return Json(new { success = false, message = "Session expired" });
        }

        // Find the enrollment record for this student+course
        var enrollment = await _context.Enrollments
            .FirstOrDefaultAsync(e => e.StudentId == studentId.Value && e.CourseId == courseId);

        if (enrollment == null)
        {
            // nothing to remove (maybe already removed)
            return Json(new { success = false, message = "Enrollment not found" });
        }

        _context.Enrollments.Remove(enrollment);
        await _context.SaveChangesAsync();

        return Json(new { success = true });
    }








    // GET: Show list of courses as checkboxes




    public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["Success"] = "Logged out successfully!";
            return RedirectToAction("Login");
        }



        // GET: Show list of courses
        [HttpGet]
        public async Task<IActionResult> SelectCourses()
        {
            int? studentId = HttpContext.Session.GetInt32("StudentId");

            if (studentId == null)
            {
                TempData["Error"] = "Your session has expired. Please log in again.";
                return RedirectToAction("Login");
            }

            // Load all courses
            var courses = await _context.Scourses.ToListAsync();

            // Load previously selected courses
            var selectedCourseIds = await _context.Enrollments
                .Where(e => e.StudentId == studentId)
                .Select(e => e.CourseId)
                .ToListAsync();

            ViewBag.SelectedCourses = selectedCourseIds;

            return View(courses);
        }



        // POST: Save selected courses
        [HttpPost]
        public async Task<IActionResult> SelectCourses(int[] selectedCourses)
        {
            if (selectedCourses == null || selectedCourses.Length == 0)
            {
                ViewBag.Error = "Please select at least one course.";

                var courses = await _context.Scourses.ToListAsync();
                return View(courses);
            }

            int? studentId = HttpContext.Session.GetInt32("StudentId");

            if (studentId == null)
            {
                TempData["Error"] = "Your session has expired. Please log in again.";
                return RedirectToAction("Login");
            }

            // Get student's already selected courses
            var existingCourses = await _context.Enrollments
                .Where(e => e.StudentId == studentId)
                .Select(e => e.CourseId)
                .ToListAsync();

            // Save only the new ones
            foreach (var courseId in selectedCourses)
            {
                if (!existingCourses.Contains(courseId))
                {
                    _context.Enrollments.Add(new Enrollment
                    {
                        StudentId = studentId.Value,
                        CourseId = courseId
                    });
                }
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "✅ Courses selected successfully!";
            return RedirectToAction("Dashboard");
        }




        // ------------------ EDIT PROFILE (GET) ------------------




        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            string? email = HttpContext.Session.GetString("Email");

            if (string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "Session expired!";
                return RedirectToAction("Login");
            }

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.Email == email);

            if (student == null)
            {
                TempData["Error"] = "Profile not found!";
                return RedirectToAction("Dashboard");
            }

            return View(student);
        }


        // ============================================
        //  ✅ EDIT PROFILE (POST)
        // ============================================
        [HttpPost]
        public async Task<IActionResult> EditProfile(Student model, IFormFile? Avatar, string[] Hobbies)
        {
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.StudentId == model.StudentId);

            if (student == null)
            {
                TempData["Error"] = "Student not found!";
                return RedirectToAction("Dashboard");
            }

            // Update fields
            student.FirstName = model.FirstName;
            student.LastName = model.LastName;
            student.PhoneNumber = model.PhoneNumber;
            student.Country = model.Country;
            student.State = model.State;
            student.City = model.City;
            student.DOB = model.DOB;

            // Save Hobbies (convert list to CSV string)
            student.Hobbies = string.Join(",", Hobbies);

            // Avatar upload
            if (Avatar != null)
            {
                string folder = "wwwroot/uploads/";
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                string filePath = Path.Combine(folder, Avatar.FileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    Avatar.CopyTo(stream);
                }

                student.AvatarPath = "/uploads/" + Avatar.FileName;
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Profile updated successfully!";
            return RedirectToAction("Dashboard");
        }


















        [HttpPost]
        public IActionResult DeleteCourse(int courseId)
        {
            int? studentId = HttpContext.Session.GetInt32("StudentId");

            if (studentId == null)
                return Json(new { success = false });

            var enrollment = _context.Enrollments
                .FirstOrDefault(e => e.StudentId == studentId && e.CourseId == courseId);

            if (enrollment != null)
            {
                _context.Enrollments.Remove(enrollment);
                _context.SaveChanges();
            }

            return Json(new { success = true });
        }



















        //        // ✅ Dynamic dropdowns
        //        public JsonResult GetStates(string country)
        //        {
        //            var states = country switch
        //            {
        //                "India" => new[] { "Kerala", "Tamil Nadu", "Karnataka" },
        //                "USA" => new[] { "California", "Texas", "Florida" },
        //                "UK" => new[] { "England", "Scotland", "Wales" },
        //                _ => Array.Empty<string>()
        //            };
        //            return Json(states);
        //        }

        //        public JsonResult GetCities(string state)
        //        {
        //            var cities = state switch
        //            {
        //                "Kerala" => new[] { "Trivandrum", "Kochi", "Kozhikode" },
        //                "Tamil Nadu" => new[] { "Chennai", "Coimbatore", "Madurai" },
        //                "Karnataka" => new[] { "Bangalore", "Mysore", "Mangalore" },
        //                "California" => new[] { "Los Angeles", "San Diego" },
        //                "Texas" => new[] { "Houston", "Dallas" },
        //                "England" => new[] { "Manchester", "Liverpool" },
        //                "Scotland" => new[] { "Amsterdam", "" },
        //                _ => Array.Empty<string>()
        //            };
        //            return Json(cities);
        //        }
        //    }
        //}




        //  ✅ Dynamic Dropdowns (Country → State → City)
        // =============================================================

      public JsonResult GetStates(string country)
{
    var states = country switch
    {
        "India" => new[] { "Kerala", "Tamil Nadu", "Karnataka" },
        "USA" => new[] { "California", "Texas", "Florida" },
        "UK" => new[] { "England", "Scotland", "Wales" },
        _ => Array.Empty<string>()
    };
    return Json(states);
}

public JsonResult GetCities(string state)
{
    var cities = state switch
    {
        "Kerala" => new[] { "Trivandrum", "Kochi", "Kozhikode" },
        "Tamil Nadu" => new[] { "Chennai", "Coimbatore", "Madurai" },
        "Karnataka" => new[] { "Bangalore", "Mysore", "Mangalore" },

        "California" => new[] { "Los Angeles", "San Diego" },
        "Texas" => new[] { "Houston", "Dallas" },

        "England" => new[] { "London", "Manchester", "Liverpool" },
        "Scotland" => new[] { "Edinburgh", "Glasgow" },
        "Wales" => new[] { "Cardiff", "Swansea" },

        _ => Array.Empty<string>()
    };

    return Json(cities);
}

    }
}