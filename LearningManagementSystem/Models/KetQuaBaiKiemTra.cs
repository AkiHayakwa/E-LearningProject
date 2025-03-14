using System.ComponentModel.DataAnnotations;

namespace LearningManagementSystem.Models
{
    public class KetQuaBaiKiemTra
    {
        [Key]
        public int MaKetQua { get; set; }

        [Required]
        public int MaBaiKiemTra { get; set; }

        [Required]
        public int MaNguoiDung { get; set; }

        [Required, Range(0, double.MaxValue)]
        public float DiemSo { get; set; }

        public DateTime NgayNop { get; set; } = DateTime.Now;

        // Navigation properties
        public BaiKiemTra? BaiKiemTra { get; set; }
        public NguoiDung? NguoiDung { get; set; }
    }
}
