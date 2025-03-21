using System.ComponentModel.DataAnnotations;

namespace LearningManagementSystem.Models
{
    public class Lesson
    {
        [Key]
        [Required, StringLength(50)]
        public string LessonId { get; set; }

        [Required, StringLength(50)]
        public string CourseId { get; set; }

        [Required, StringLength(100)]
        public string LessonTitle { get; set; }

        public string Content { get; set; }

        public int OrderNumber { get; set; }

        // Navigation properties
        public Course Course { get; set; }

        public List<Comment> Comments { get; set; } = new List<Comment>();
        public List<Progress> Progresses { get; set; }
    }
}
