using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore; // ✅ Add this at the top


namespace ScopeIndia.Models
{
    // ===================== STUDENT MODEL =====================

    public class Student
    {
        [Key]
        public int StudentId { get; set; }

        [Required]
        public string? FirstName { get; set; }

        [Required]
        public string? LastName { get; set; }

        [Required, EmailAddress]
        public string? Email { get; set; }

        public string? Username { get; set; }
        public string? Password { get; set; }

        [Required(ErrorMessage = "Please select your gender")]
        public string? Gender { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DOB { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }

        // 🔹 These can be NULL
        public string? Address { get; set; }

        // 🔹 Make location fields nullable
        public string? Country { get; set; }
        public string? State { get; set; }
        public string? City { get; set; }

        public string? Hobbies { get; set; }
        public string? AvatarPath { get; set; }

        // 🔹 OTP Features (NULL allowed)
        public string? TempPassword { get; set; }
        public string? OTP { get; set; }
        public DateTime? OtpExpireTime { get; set; }

        public List<StudentCourse>? StudentCourses { get; set; }
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }



















    // ===================== COURSE MODEL =====================


    public class Course
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string CourseName { get; set; }

        [Required]
        public string Duration { get; set; }

        [Precision(18, 2)] // ✅ Fix decimal precision (18 digits, 2 decimal places)
        public decimal Fee { get; set; }

        public List<StudentCourse>? StudentCourses { get; set; }
    }


    // ===================== STUDENT-COURSE RELATION =====================
    public class StudentCourse
    {
        [Key]
        public int Id { get; set; }

        public int StudentId { get; set; }
        public Student? Student { get; set; }

        public int CourseId { get; set; }
        public Course? Course { get; set; }
    }

    // ===================== DASHBOARD VIEW MODEL =====================
    public class StudentDashboardViewModel
    {
        public string? StudentName { get; set; }
        public string? Email { get; set; }
        public string? AvatarPath { get; set; }
        public string? Hobbies { get; set; }
        public List<Course>? Courses { get; set; }
        public List<Course>? MyCourses { get; set; }
    }
}