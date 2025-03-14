using System.ComponentModel.DataAnnotations;

namespace LearningManagementSystem.Models
{
    public class ThanhVienNhom
    {
        [Key]
        public int MaThanhVienNhom { get; set; }

        [Required]
        public int MaNhom { get; set; }

        [Required]
        public int MaNguoiDung { get; set; }

        public DateTime NgayThamGia { get; set; } = DateTime.Now;

        // Navigation properties
        public Nhom? Nhom { get; set; }
        public NguoiDung? NguoiDung { get; set; }
    }
}
