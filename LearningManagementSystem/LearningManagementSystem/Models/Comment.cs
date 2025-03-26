using System.ComponentModel.DataAnnotations;

namespace LearningManagementSystem.Models
{
    public class Comment
    {
        [Key]
        [Required, StringLength(50)]
        public string CommentId { get; set; }

        [Required, StringLength(50)]
        public string CourseId { get; set; }

        [Required, StringLength(50)]
        public string UserName { get; set; }

        [Required, StringLength(1000)]
        public string Content { get; set; }

        public DateTime CreatedDate { get; set; }

        public Course Course { get; set; }
        public User User { get; set; }
    }
}