using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ScopeIndia.Models
{
    [Table("Enrollments")]
    public class Enrollment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        public int CourseId { get; set; }

        [ForeignKey("StudentId")]
        public Student? Student { get; set; }

        [ForeignKey("CourseId")]
        public Scourse? Course { get; set; }

        public DateTime EnrolledOn { get; set; } = DateTime.Now;
    }
}