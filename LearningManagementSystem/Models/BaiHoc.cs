using System.ComponentModel.DataAnnotations;

namespace LearningManagementSystem.Models
{
    public class BaiHoc
    {
        [Key]
        public int MaBaiHoc { get; set; }

        [Required]
        public int MaKhoaHoc { get; set; }

        [Required, StringLength(200)]
        public string TenBaiHoc { get; set; }

        [Required, StringLength(10)]
        public string LoaiNoiDung { get; set; } // "video", "pdf", "van_ban", "bai_kiem_tra"

        [StringLength(255)]
        public string DuongDanNoiDung { get; set; }

        public string NoiDungVanBan { get; set; }

        [Required]
        public int ThuTu { get; set; }

        // Navigation properties
        public KhoaHoc? KhoaHoc { get; set; }
        public List<TuongTac>? TuongTac { get; set; }
        public List<BaiKiemTra>? BaiKiemTra { get; set; }
        public List<DichBaiHoc>? DichBaiHoc { get; set; }
    }
}
