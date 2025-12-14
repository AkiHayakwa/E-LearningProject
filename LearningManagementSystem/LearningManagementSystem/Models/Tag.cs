using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LearningManagementSystem.Models
{
    public class Tag
    {
        [Key]
        [MaxLength(50)]
        public string TagId { get; set; } = Guid.NewGuid().ToString("N").Substring(0, 10);

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        [MaxLength(120)]
        public string? Slug { get; set; }

        [Required]
        [MaxLength(50)]
        public string Category { get; set; } = "General";

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(50)]
        public string? CreatedBy { get; set; }

        public ICollection<CourseTag> CourseTags { get; set; } = new List<CourseTag>();
    }
}

