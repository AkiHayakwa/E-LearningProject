using System.ComponentModel.DataAnnotations;

namespace LearningManagementSystem.Models
{
    public class Course
    {
        [Key]
        [Required, StringLength(50)]
        public string CourseId { get; set; }

        [Required, StringLength(100)]
        public string CourseName { get; set; }

        [Required, StringLength(500)]
        public string Description { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [StringLength(50)]
        public string InstructorId { get; set; }

        public User Instructor { get; set; }
        public List<Lesson> Lessons { get; set; }
        public List<Enrollment> Enrollments { get; set; }
    }
}
