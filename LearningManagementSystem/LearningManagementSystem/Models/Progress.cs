using System.ComponentModel.DataAnnotations;

namespace LearningManagementSystem.Models
{
    public class Progress
    {
        [Key]
        [Required, StringLength(50)]
        public string ProgressId { get; set; }

        [Required, StringLength(50)]
        public string UserId { get; set; }

        [Required, StringLength(50)]
        public string LessonId { get; set; }

        public bool CompletionStatus { get; set; } = false;

        public DateTime? CompletionDate { get; set; }

        public User User { get; set; }
        public Lesson Lesson { get; set; }
    }
}
