using System.ComponentModel.DataAnnotations;

namespace LearningManagementSystem.Models
{
    public class XepHang
    {
        [Key]
        public int MaXepHang { get; set; }

        [Required]
        public int MaNguoiDung { get; set; }

        [Required]
        public int MaKhoaHoc { get; set; }

        public int DiemSo { get; set; } = 0;

        public int? ViTriXepHang { get; set; }

        public DateTime NgayCapNhat { get; set; } = DateTime.Now;

        // Navigation properties
        public NguoiDung? NguoiDung { get; set; }
        public KhoaHoc? KhoaHoc { get; set; }
    }
}
