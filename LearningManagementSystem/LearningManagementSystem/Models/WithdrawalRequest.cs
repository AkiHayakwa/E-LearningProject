using System.ComponentModel.DataAnnotations;

namespace LearningManagementSystem.Models
{
    public class WithdrawalRequest
    {
        [Key]
        [Required]
        [StringLength(50)]
        public string WithdrawalRequestId { get; set; }

        [Required]
        [StringLength(50)]
        public string UserName { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(100)]
        public string BankName { get; set; }

        [Required]
        [StringLength(20)]
        public string AccountNumber { get; set; }

        [StringLength(100)]
        public string? AccountHolderName { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } // "Pending", "Approved", "Rejected"

        [StringLength(1000)]
        public string? AdminNote { get; set; }

        [Required]
        public DateTime RequestDate { get; set; }

        public DateTime? ProcessedDate { get; set; }

        [StringLength(50)]
        public string? ProcessedBy { get; set; }

        // Navigation properties
        public User? User { get; set; }
        public User? ProcessedByUser { get; set; }
    }
}

