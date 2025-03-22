using System.ComponentModel.DataAnnotations;

namespace LearningManagementSystem.Models
{
    public class Comment
    {
        [Key]
        [Required, StringLength(50)]
        public string CommentId { get; set; }

        [Required, StringLength(50)]
        public string LessonId { get; set; }

        public string CourseId  { get; set; }

        [Required, StringLength(50)]
        public string UserId { get; set; }

        [Required, StringLength(1000)]
        public string Content { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation properties
        public Lesson Lesson { get; set; }
        public User User { get; set; }
    }
}