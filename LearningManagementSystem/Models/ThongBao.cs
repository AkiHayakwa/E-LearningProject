using System.ComponentModel.DataAnnotations;

namespace LearningManagementSystem.Models
{
    public class ThongBao
    {
        [Key]
        public int MaThongBao { get; set; }

        [Required]
        public int MaNguoiDung { get; set; }

        [Required, StringLength(200)]
        public string TieuDe { get; set; }

        public string NoiDung { get; set; }

        public bool DaDoc { get; set; } = false;

        public DateTime NgayTao { get; set; } = DateTime.Now;

        // Navigation properties
        public NguoiDung? NguoiDung { get; set; }
    }
}
