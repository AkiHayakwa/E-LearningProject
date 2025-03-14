using LearningManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace LearningManagementSystem.Data
{
    public class LMSContext :DbContext
    {
        public LMSContext(DbContextOptions<LMSContext> options) : base(options)
        {
        }

        public DbSet<NguoiDung> NguoiDung { get; set; }
        public DbSet<KhoaHoc> KhoaHoc { get; set; }
        public DbSet<DangKyKhoaHoc> DangKyKhoaHoc { get; set; }
        public DbSet<BaiHoc> BaiHoc { get; set; }
        public DbSet<TuongTac> TuongTac { get; set; }
        public DbSet<BaiKiemTra> BaiKiemTra { get; set; }
        public DbSet<KetQuaBaiKiemTra> KetQuaBaiKiemTra { get; set; }
        public DbSet<NgonNgu> NgonNgu { get; set; }
        public DbSet<DichKhoaHoc> DichKhoaHoc { get; set; }
        public DbSet<DichBaiHoc> DichBaiHoc { get; set; }
        public DbSet<ThongBao> ThongBao { get; set; }
        public DbSet<LichSuDangNhap> LichSuDangNhap { get; set; }
        public DbSet<DanhMuc> DanhMuc { get; set; }
        public DbSet<LichTrinh> LichTrinh { get; set; }
        public DbSet<Nhom> Nhom { get; set; }
        public DbSet<ThanhVienNhom> ThanhVienNhom { get; set; }
        public DbSet<XepHang> XepHang { get; set; }
        public DbSet<NhatKyHoatDong> NhatKyHoatDong { get; set; }
    }
}
