using System.ComponentModel.DataAnnotations;

namespace LearningManagementSystem.Models
{
    public class Enrollment
    {
        [Key]
        [Required, StringLength(50)]
        public string EnrollmentId { get; set; }

        [Required, StringLength(50)]
        public string UserId { get; set; }

        [Required, StringLength(50)]
        public string CourseId { get; set; }

        public DateTime EnrollmentDate { get; set; } = DateTime.Now;

        public User User { get; set; }
        public Course Course { get; set; }
    }
}
