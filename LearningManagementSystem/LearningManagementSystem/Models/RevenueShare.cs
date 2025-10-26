using System.ComponentModel.DataAnnotations;

namespace LearningManagementSystem.Models
{
    public class RevenueShare
    {
        [Key]
        [Required]
        [StringLength(50)]
        public string RevenueShareId { get; set; }

        [Required]
        [StringLength(50)]
        public string PaymentId { get; set; }

        [Required]
        [StringLength(50)]
        public string UserName { get; set; }

        [Required]
        [StringLength(50)]
        public string CourseId { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        [Range(0, 100)]
        public decimal Percentage { get; set; }

        [Required]
        [StringLength(50)]
        public string ShareType { get; set; } // "Instructor" hoặc "Admin"

        [Required]
        public DateTime CreatedDate { get; set; }

        // Navigation properties
        public Payment? Payment { get; set; }
        public User? User { get; set; }
        public Course? Course { get; set; }
    }
}

