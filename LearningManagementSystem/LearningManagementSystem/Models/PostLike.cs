using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LearningManagementSystem.Models
{
    public class PostLike
    {
        [Key]
        [Required]
        [StringLength(50)]
        public string LikeId { get; set; }

        [Required]
        [StringLength(50)]
        public string PostId { get; set; }

        [Required]
        [StringLength(50)]
        public string UserName { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        // Navigation properties
        public Post? Post { get; set; }
        public User? User { get; set; }
    }
}

