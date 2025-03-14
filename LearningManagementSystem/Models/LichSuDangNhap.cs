using System.ComponentModel.DataAnnotations;

namespace LearningManagementSystem.Models
{
    public class LichSuDangNhap
    {
        [Key]
        public int MaDangNhap { get; set; }

        [Required]
        public int MaNguoiDung { get; set; }

        public DateTime ThoiGianDangNhap { get; set; } = DateTime.Now;

        [StringLength(45)]
        public string DiaChiIp { get; set; }

        [StringLength(255)]
        public string ThongTinThietBi { get; set; }

        // Navigation properties
        public NguoiDung? NguoiDung { get; set; }
    }
}
