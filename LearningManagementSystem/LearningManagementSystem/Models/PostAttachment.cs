using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LearningManagementSystem.Models
{
    public class PostAttachment
    {
        [Key]
        [Required]
        [StringLength(50)]
        public string AttachmentId { get; set; }

        [Required]
        [StringLength(50)]
        public string PostId { get; set; }

        [Required]
        [StringLength(255)]
        public string FileName { get; set; }

        [Required]
        [StringLength(500)]
        public string FilePath { get; set; }

        [Required]
        public long FileSize { get; set; }

        [StringLength(50)]
        public string? FileType { get; set; }

        [Required]
        public DateTime UploadedDate { get; set; }

        // Navigation properties
        public Post? Post { get; set; }
    }
}

