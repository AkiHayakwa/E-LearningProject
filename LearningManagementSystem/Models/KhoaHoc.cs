using System.ComponentModel.DataAnnotations;

namespace LearningManagementSystem.Models
{
    public class KhoaHoc
    {
        [Key]
        public int MaKhoaHoc { get; set; }

        [Required, StringLength(200)]
        public string TenKhoaHoc { get; set; }

        public string MoTa { get; set; }

        [Required]
        public int MaGiaoVien { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.Now;
        public DateTime NgayCapNhat { get; set; } = DateTime.Now;

        public int? MaDanhMuc { get; set; }

        // Navigation properties
        public NguoiDung? GiaoVien { get; set; }
        public DanhMuc? DanhMuc { get; set; }
        public List<BaiHoc>? BaiHoc { get; set; }
        public List<DangKyKhoaHoc>? DangKyKhoaHoc { get; set; }
        public List<TuongTac>? TuongTac { get; set; }
        public List<DichKhoaHoc>? DichKhoaHoc { get; set; }
        public List<LichTrinh>? LichTrinh { get; set; }
        public List<Nhom>? Nhom { get; set; }
        public List<XepHang>? XepHang { get; set; }
    }
}
