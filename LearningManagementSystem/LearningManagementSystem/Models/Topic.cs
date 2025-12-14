using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LearningManagementSystem.Models
{
    public class Topic
    {
        [Key]
        [Required]
        [StringLength(50)]
        public string TopicId { get; set; }

        [Required]
        [StringLength(50)]
        public string ForumId { get; set; }

        [Required]
        [StringLength(50)]
        public string UserName { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        [StringLength(10000)]
        public string Content { get; set; }

        [Required]
        public int ViewCount { get; set; } = 0;

        [Required]
        public int ReplyCount { get; set; } = 0;

        public DateTime? LastReplyDate { get; set; }

        [StringLength(50)]
        public string? LastReplyBy { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "New"; // New, Hot, Solved, Locked, Pinned

        [Required]
        public DateTime CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public Forum? Forum { get; set; }
        public User? User { get; set; }
        public List<Post>? Posts { get; set; }
        public List<TopicTag>? TopicTags { get; set; }
    }
}

