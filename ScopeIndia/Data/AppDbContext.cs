using Microsoft.EntityFrameworkCore;
using ScopeIndia.Models;

namespace ScopeIndia.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // ✅ Tables
        public DbSet<Student> Students { get; set; }
        public DbSet<Login> Logins { get; set; }
        public DbSet<Scourse> Scourses { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ✅ Decimal precision fix
            modelBuilder.Entity<Scourse>()
                .Property(c => c.Fee)
                .HasPrecision(18, 2);

            // ✅ Relationships
            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Student)
                .WithMany(s => s.Enrollments)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}