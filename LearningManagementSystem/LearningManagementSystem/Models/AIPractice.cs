using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LearningManagementSystem.Models
{
    public class AIPractice
    {
        [Key]
        [MaxLength(50)]
        public string Id { get; set; } = Guid.NewGuid().ToString("N").Substring(0, 10);

        [Required]
        [MaxLength(50)]
        public string UserName { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string CourseId { get; set; } = null!;

        [Required]
        public string Question { get; set; } = null!;

        [Required]
        public string Options { get; set; } = null!; // JSON: ["A. SELECT *", "B. INSERT"]

        [Required]
        [MaxLength(10)]
        public string CorrectAnswer { get; set; } = null!; // "A", "B",...

        [MaxLength(10)]
        public string? UserAnswer { get; set; }

        public bool? IsCorrect { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? AnsweredAt { get; set; }

        // Navigation
        public User User { get; set; } = null!;
        public Course Course { get; set; } = null!;
    }
}