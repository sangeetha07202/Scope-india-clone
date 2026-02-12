using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ScopeIndia.Models
{
    [Table("Scourses")] // or "Courses" depending on your DB name
    public class Scourse
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string CourseName { get; set; } = string.Empty;

        [Required]
        public string Duration { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Fee { get; set; }

        // Navigation
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}