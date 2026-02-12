using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ScopeIndia.Models
{
    public class StudentLogin
    {
        [Key]
        public int LoginId { get; set; }

        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [NotMapped]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;

        // 🔹 Foreign key reference to Student table
        public int StudentId { get; set; }

        [ForeignKey("StudentId")]
        public Student? Student { get; set; }
    }
}