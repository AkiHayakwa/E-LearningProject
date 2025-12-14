using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LearningManagementSystem.Models
{
    public class Report
    {
        [Key]
        [Required]
        [StringLength(50)]
        public string ReportId { get; set; }

        [StringLength(50)]
        public string? PostId { get; set; } // Có thể báo cáo post

        [StringLength(50)]
        public string? TopicId { get; set; } // Hoặc báo cáo topic

        [Required]
        [StringLength(50)]
        public string ReportedBy { get; set; } // UserName người báo cáo

        [Required]
        [StringLength(50)]
        public string Reason { get; set; } // Spam, Inappropriate, Harassment, Copyright, Other

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Resolved, Rejected

        [StringLength(50)]
        public string? ResolvedBy { get; set; } // UserName admin xử lý

        public DateTime? ResolvedDate { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        // Navigation properties
        public Post? Post { get; set; }
        public Topic? Topic { get; set; }
        public User? ReportedByUser { get; set; }
        public User? ResolvedByUser { get; set; }
    }
}

