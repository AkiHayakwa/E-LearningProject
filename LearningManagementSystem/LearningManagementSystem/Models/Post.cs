using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LearningManagementSystem.Models
{
    public class Post
    {
        [Key]
        [Required]
        [StringLength(50)]
        public string PostId { get; set; }

        [Required]
        [StringLength(50)]
        public string TopicId { get; set; }

        [Required]
        [StringLength(50)]
        public string UserName { get; set; }

        [Required]
        [StringLength(10000)]
        public string Content { get; set; }

        [StringLength(50)]
        public string? ParentPostId { get; set; } // NULL nếu là reply cho topic, có giá trị nếu reply cho post khác

        [Required]
        public int LikeCount { get; set; } = 0;

        [Required]
        public bool IsAnswer { get; set; } = false; // Có phải câu trả lời chính xác không

        [Required]
        public DateTime CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public Topic? Topic { get; set; }
        public User? User { get; set; }
        public Post? ParentPost { get; set; }
        public List<Post>? Replies { get; set; }
        public List<PostLike>? PostLikes { get; set; }
        public List<PostAttachment>? PostAttachments { get; set; }
    }
}

