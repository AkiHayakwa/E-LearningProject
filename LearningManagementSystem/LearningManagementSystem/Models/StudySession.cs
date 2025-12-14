using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LearningManagementSystem.Models
{
    public class StudySession
    {
        [Key]
        [MaxLength(50)]
        public string SessionId { get; set; } = Guid.NewGuid().ToString("N").Substring(0, 10);

        [Required]
        [MaxLength(50)]
        public string UserName { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string CourseId { get; set; } = null!;

        [MaxLength(50)]
        public string? LessonId { get; set; }

        [Required]
        public DateTime StartedAt { get; set; }

        public DateTime? EndedAt { get; set; }

        /// <summary>
        /// Tổng thời gian học (giây)
        /// </summary>
        [Required]
        public int DurationSeconds { get; set; }

        [MaxLength(200)]
        public string? Note { get; set; }

        // Navigation
        public User User { get; set; } = null!;
        public Course Course { get; set; } = null!;
        public Lesson? Lesson { get; set; }
    }
}

