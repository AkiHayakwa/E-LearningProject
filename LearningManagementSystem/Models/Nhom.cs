using System.ComponentModel.DataAnnotations;

namespace LearningManagementSystem.Models
{
    public class Nhom
    {
        [Key]
        public int MaNhom { get; set; }

        [Required]
        public int MaKhoaHoc { get; set; }

        [Required, StringLength(100)]
        public string TenNhom { get; set; }

        public string MoTa { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.Now;

        // Navigation properties
        public KhoaHoc? KhoaHoc { get; set; }
        public List<ThanhVienNhom>? ThanhVienNhom { get; set; }
    }
}
