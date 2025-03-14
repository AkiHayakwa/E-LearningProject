using System.ComponentModel.DataAnnotations;

namespace LearningManagementSystem.Models
{
    public class NguoiDung
    {
        [Key]
        public int MaNguoiDung { get; set; }

        [Required, StringLength(50)]
        public string TenDangNhap { get; set; }

        [Required, StringLength(255)]
        public string MatKhau { get; set; }

        [Required, StringLength(100), EmailAddress]
        public string Email { get; set; }

        [StringLength(100)]
        public string HoTen { get; set; }

        [Required, StringLength(10)]
        public string VaiTro { get; set; } // "quan_tri", "giao_vien", "hoc_sinh"

        public DateTime NgayTao { get; set; } = DateTime.Now;
        public DateTime NgayCapNhat { get; set; } = DateTime.Now;

        // Navigation properties (nullable và không dùng virtual)
        public List<KhoaHoc>? KhoaHocGiaoVien { get; set; }
        public List<DangKyKhoaHoc>? DangKyKhoaHoc { get; set; }
        public List<TuongTac>? TuongTac { get; set; }
        public List<KetQuaBaiKiemTra>? KetQuaBaiKiemTra { get; set; }
        public List<ThongBao>? ThongBao { get; set; }
        public List<LichSuDangNhap>? LichSuDangNhap { get; set; }
        public List<ThanhVienNhom>? ThanhVienNhom { get; set; }
        public List<XepHang>? XepHang { get; set; }
        public List<NhatKyHoatDong>? NhatKyHoatDong { get; set; }
    }
}
