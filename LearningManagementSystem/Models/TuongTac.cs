using System.ComponentModel.DataAnnotations;

namespace LearningManagementSystem.Models
{
    public class TuongTac
    {
        [Key]
        public int MaTuongTac { get; set; }

        [Required]
        public int MaNguoiDung { get; set; }

        public int? MaKhoaHoc { get; set; }

        public int? MaBaiHoc { get; set; }

        [Required]
        public string TinNhan { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.Now;

        // Navigation properties
        public NguoiDung? NguoiDung { get; set; }
        public KhoaHoc? KhoaHoc { get; set; }
        public BaiHoc? BaiHoc { get; set; }
    }
}
