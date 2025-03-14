using System.ComponentModel.DataAnnotations;

namespace LearningManagementSystem.Models
{
    public class DangKyKhoaHoc
    {
        [Key]
        public int MaDangKy { get; set; }

        [Required]
        public int MaNguoiDung { get; set; }

        [Required]
        public int MaKhoaHoc { get; set; }

        public DateTime NgayDangKy { get; set; } = DateTime.Now;

        [Range(0, 100)]
        public float TienDo { get; set; } = 0;

        // Navigation properties
        public NguoiDung? NguoiDung { get; set; }
        public KhoaHoc? KhoaHoc { get; set; }
    }
}
