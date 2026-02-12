using System.ComponentModel.DataAnnotations;

namespace ScopeIndia.Models
{
    public class Login
    {
        [Key]
        public int Id { get; set; }

        // --- Common login fields ---
        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string? PasswordHash { get; set; }

        public bool RememberMe { get; set; }

        // --- Optional fields for Forgot / Reset Password ---
        public bool IsForgotPassword { get; set; } = false;
        public bool IsResetPassword { get; set; } = false;

        [DataType(DataType.Password)]
        public string? TempPassword { get; set; }

        [DataType(DataType.Password)]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        public string? ConfirmPassword { get; set; }

        // --- System columns ---
        public bool IsFirstLogin { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

       
        public string UserName { get; set; } // ✅ match this with controller
        public string Password { get; set; }

    }
}