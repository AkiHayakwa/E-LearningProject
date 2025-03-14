using System.ComponentModel.DataAnnotations;

namespace LearningManagementSystem.Models
{
    public class NhatKyHoatDong
    {
        [Key]
        public int MaNhatKy { get; set; }

        [Required]
        public int MaNguoiDung { get; set; }

        [Required, StringLength(100)]
        public string HanhDong { get; set; }

        public string MoTa { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.Now;

        // Navigation properties
        public NguoiDung? NguoiDung { get; set; }
    }
}
