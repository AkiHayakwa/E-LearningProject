using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LearningManagementSystem.Models
{
    public class Forum
    {
        [Key]
        [Required]
        [StringLength(50)]
        public string ForumId { get; set; }

        [StringLength(50)]
        public string? CourseId { get; set; } // NULL nếu là diễn đàn chung

        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public Course? Course { get; set; }
        public List<Topic>? Topics { get; set; }
    }
}

