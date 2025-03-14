using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class v1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DanhMuc",
                columns: table => new
                {
                    MaDanhMuc = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenDanhMuc = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanhMuc", x => x.MaDanhMuc);
                });

            migrationBuilder.CreateTable(
                name: "NgonNgu",
                columns: table => new
                {
                    MaNgonNgu = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaCodeNgonNgu = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TenNgonNgu = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NgonNgu", x => x.MaNgonNgu);
                });

            migrationBuilder.CreateTable(
                name: "NguoiDung",
                columns: table => new
                {
                    MaNguoiDung = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenDangNhap = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MatKhau = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    VaiTro = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NguoiDung", x => x.MaNguoiDung);
                });

            migrationBuilder.CreateTable(
                name: "KhoaHoc",
                columns: table => new
                {
                    MaKhoaHoc = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenKhoaHoc = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaGiaoVien = table.Column<int>(type: "int", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MaDanhMuc = table.Column<int>(type: "int", nullable: true),
                    GiaoVienMaNguoiDung = table.Column<int>(type: "int", nullable: true),
                    DanhMucMaDanhMuc = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KhoaHoc", x => x.MaKhoaHoc);
                    table.ForeignKey(
                        name: "FK_KhoaHoc_DanhMuc_DanhMucMaDanhMuc",
                        column: x => x.DanhMucMaDanhMuc,
                        principalTable: "DanhMuc",
                        principalColumn: "MaDanhMuc");
                    table.ForeignKey(
                        name: "FK_KhoaHoc_NguoiDung_GiaoVienMaNguoiDung",
                        column: x => x.GiaoVienMaNguoiDung,
                        principalTable: "NguoiDung",
                        principalColumn: "MaNguoiDung");
                });

            migrationBuilder.CreateTable(
                name: "LichSuDangNhap",
                columns: table => new
                {
                    MaDangNhap = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaNguoiDung = table.Column<int>(type: "int", nullable: false),
                    ThoiGianDangNhap = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DiaChiIp = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: false),
                    ThongTinThietBi = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    NguoiDungMaNguoiDung = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LichSuDangNhap", x => x.MaDangNhap);
                    table.ForeignKey(
                        name: "FK_LichSuDangNhap_NguoiDung_NguoiDungMaNguoiDung",
                        column: x => x.NguoiDungMaNguoiDung,
                        principalTable: "NguoiDung",
                        principalColumn: "MaNguoiDung");
                });

            migrationBuilder.CreateTable(
                name: "NhatKyHoatDong",
                columns: table => new
                {
                    MaNhatKy = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaNguoiDung = table.Column<int>(type: "int", nullable: false),
                    HanhDong = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NguoiDungMaNguoiDung = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhatKyHoatDong", x => x.MaNhatKy);
                    table.ForeignKey(
                        name: "FK_NhatKyHoatDong_NguoiDung_NguoiDungMaNguoiDung",
                        column: x => x.NguoiDungMaNguoiDung,
                        principalTable: "NguoiDung",
                        principalColumn: "MaNguoiDung");
                });

            migrationBuilder.CreateTable(
                name: "ThongBao",
                columns: table => new
                {
                    MaThongBao = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaNguoiDung = table.Column<int>(type: "int", nullable: false),
                    TieuDe = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DaDoc = table.Column<bool>(type: "bit", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NguoiDungMaNguoiDung = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThongBao", x => x.MaThongBao);
                    table.ForeignKey(
                        name: "FK_ThongBao_NguoiDung_NguoiDungMaNguoiDung",
                        column: x => x.NguoiDungMaNguoiDung,
                        principalTable: "NguoiDung",
                        principalColumn: "MaNguoiDung");
                });

            migrationBuilder.CreateTable(
                name: "BaiHoc",
                columns: table => new
                {
                    MaBaiHoc = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaKhoaHoc = table.Column<int>(type: "int", nullable: false),
                    TenBaiHoc = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    LoaiNoiDung = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DuongDanNoiDung = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    NoiDungVanBan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ThuTu = table.Column<int>(type: "int", nullable: false),
                    KhoaHocMaKhoaHoc = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaiHoc", x => x.MaBaiHoc);
                    table.ForeignKey(
                        name: "FK_BaiHoc_KhoaHoc_KhoaHocMaKhoaHoc",
                        column: x => x.KhoaHocMaKhoaHoc,
                        principalTable: "KhoaHoc",
                        principalColumn: "MaKhoaHoc");
                });

            migrationBuilder.CreateTable(
                name: "DangKyKhoaHoc",
                columns: table => new
                {
                    MaDangKy = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaNguoiDung = table.Column<int>(type: "int", nullable: false),
                    MaKhoaHoc = table.Column<int>(type: "int", nullable: false),
                    NgayDangKy = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TienDo = table.Column<float>(type: "real", nullable: false),
                    NguoiDungMaNguoiDung = table.Column<int>(type: "int", nullable: true),
                    KhoaHocMaKhoaHoc = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DangKyKhoaHoc", x => x.MaDangKy);
                    table.ForeignKey(
                        name: "FK_DangKyKhoaHoc_KhoaHoc_KhoaHocMaKhoaHoc",
                        column: x => x.KhoaHocMaKhoaHoc,
                        principalTable: "KhoaHoc",
                        principalColumn: "MaKhoaHoc");
                    table.ForeignKey(
                        name: "FK_DangKyKhoaHoc_NguoiDung_NguoiDungMaNguoiDung",
                        column: x => x.NguoiDungMaNguoiDung,
                        principalTable: "NguoiDung",
                        principalColumn: "MaNguoiDung");
                });

            migrationBuilder.CreateTable(
                name: "DichKhoaHoc",
                columns: table => new
                {
                    MaDich = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaKhoaHoc = table.Column<int>(type: "int", nullable: false),
                    MaNgonNgu = table.Column<int>(type: "int", nullable: false),
                    TenKhoaHoc = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KhoaHocMaKhoaHoc = table.Column<int>(type: "int", nullable: true),
                    NgonNguMaNgonNgu = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DichKhoaHoc", x => x.MaDich);
                    table.ForeignKey(
                        name: "FK_DichKhoaHoc_KhoaHoc_KhoaHocMaKhoaHoc",
                        column: x => x.KhoaHocMaKhoaHoc,
                        principalTable: "KhoaHoc",
                        principalColumn: "MaKhoaHoc");
                    table.ForeignKey(
                        name: "FK_DichKhoaHoc_NgonNgu_NgonNguMaNgonNgu",
                        column: x => x.NgonNguMaNgonNgu,
                        principalTable: "NgonNgu",
                        principalColumn: "MaNgonNgu");
                });

            migrationBuilder.CreateTable(
                name: "LichTrinh",
                columns: table => new
                {
                    MaLichTrinh = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaKhoaHoc = table.Column<int>(type: "int", nullable: false),
                    ThoiGianBatDau = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ThoiGianKetThuc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DiaDiem = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    KhoaHocMaKhoaHoc = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LichTrinh", x => x.MaLichTrinh);
                    table.ForeignKey(
                        name: "FK_LichTrinh_KhoaHoc_KhoaHocMaKhoaHoc",
                        column: x => x.KhoaHocMaKhoaHoc,
                        principalTable: "KhoaHoc",
                        principalColumn: "MaKhoaHoc");
                });

            migrationBuilder.CreateTable(
                name: "Nhom",
                columns: table => new
                {
                    MaNhom = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaKhoaHoc = table.Column<int>(type: "int", nullable: false),
                    TenNhom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    KhoaHocMaKhoaHoc = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Nhom", x => x.MaNhom);
                    table.ForeignKey(
                        name: "FK_Nhom_KhoaHoc_KhoaHocMaKhoaHoc",
                        column: x => x.KhoaHocMaKhoaHoc,
                        principalTable: "KhoaHoc",
                        principalColumn: "MaKhoaHoc");
                });

            migrationBuilder.CreateTable(
                name: "XepHang",
                columns: table => new
                {
                    MaXepHang = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaNguoiDung = table.Column<int>(type: "int", nullable: false),
                    MaKhoaHoc = table.Column<int>(type: "int", nullable: false),
                    DiemSo = table.Column<int>(type: "int", nullable: false),
                    ViTriXepHang = table.Column<int>(type: "int", nullable: true),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NguoiDungMaNguoiDung = table.Column<int>(type: "int", nullable: true),
                    KhoaHocMaKhoaHoc = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_XepHang", x => x.MaXepHang);
                    table.ForeignKey(
                        name: "FK_XepHang_KhoaHoc_KhoaHocMaKhoaHoc",
                        column: x => x.KhoaHocMaKhoaHoc,
                        principalTable: "KhoaHoc",
                        principalColumn: "MaKhoaHoc");
                    table.ForeignKey(
                        name: "FK_XepHang_NguoiDung_NguoiDungMaNguoiDung",
                        column: x => x.NguoiDungMaNguoiDung,
                        principalTable: "NguoiDung",
                        principalColumn: "MaNguoiDung");
                });

            migrationBuilder.CreateTable(
                name: "BaiKiemTra",
                columns: table => new
                {
                    MaBaiKiemTra = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaBaiHoc = table.Column<int>(type: "int", nullable: false),
                    TenBaiKiemTra = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DiemToiDa = table.Column<float>(type: "real", nullable: false),
                    BaiHocMaBaiHoc = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaiKiemTra", x => x.MaBaiKiemTra);
                    table.ForeignKey(
                        name: "FK_BaiKiemTra_BaiHoc_BaiHocMaBaiHoc",
                        column: x => x.BaiHocMaBaiHoc,
                        principalTable: "BaiHoc",
                        principalColumn: "MaBaiHoc");
                });

            migrationBuilder.CreateTable(
                name: "DichBaiHoc",
                columns: table => new
                {
                    MaDich = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaBaiHoc = table.Column<int>(type: "int", nullable: false),
                    MaNgonNgu = table.Column<int>(type: "int", nullable: false),
                    TenBaiHoc = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NoiDungVanBan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BaiHocMaBaiHoc = table.Column<int>(type: "int", nullable: true),
                    NgonNguMaNgonNgu = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DichBaiHoc", x => x.MaDich);
                    table.ForeignKey(
                        name: "FK_DichBaiHoc_BaiHoc_BaiHocMaBaiHoc",
                        column: x => x.BaiHocMaBaiHoc,
                        principalTable: "BaiHoc",
                        principalColumn: "MaBaiHoc");
                    table.ForeignKey(
                        name: "FK_DichBaiHoc_NgonNgu_NgonNguMaNgonNgu",
                        column: x => x.NgonNguMaNgonNgu,
                        principalTable: "NgonNgu",
                        principalColumn: "MaNgonNgu");
                });

            migrationBuilder.CreateTable(
                name: "TuongTac",
                columns: table => new
                {
                    MaTuongTac = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaNguoiDung = table.Column<int>(type: "int", nullable: false),
                    MaKhoaHoc = table.Column<int>(type: "int", nullable: true),
                    MaBaiHoc = table.Column<int>(type: "int", nullable: true),
                    TinNhan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NguoiDungMaNguoiDung = table.Column<int>(type: "int", nullable: true),
                    KhoaHocMaKhoaHoc = table.Column<int>(type: "int", nullable: true),
                    BaiHocMaBaiHoc = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TuongTac", x => x.MaTuongTac);
                    table.ForeignKey(
                        name: "FK_TuongTac_BaiHoc_BaiHocMaBaiHoc",
                        column: x => x.BaiHocMaBaiHoc,
                        principalTable: "BaiHoc",
                        principalColumn: "MaBaiHoc");
                    table.ForeignKey(
                        name: "FK_TuongTac_KhoaHoc_KhoaHocMaKhoaHoc",
                        column: x => x.KhoaHocMaKhoaHoc,
                        principalTable: "KhoaHoc",
                        principalColumn: "MaKhoaHoc");
                    table.ForeignKey(
                        name: "FK_TuongTac_NguoiDung_NguoiDungMaNguoiDung",
                        column: x => x.NguoiDungMaNguoiDung,
                        principalTable: "NguoiDung",
                        principalColumn: "MaNguoiDung");
                });

            migrationBuilder.CreateTable(
                name: "ThanhVienNhom",
                columns: table => new
                {
                    MaThanhVienNhom = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaNhom = table.Column<int>(type: "int", nullable: false),
                    MaNguoiDung = table.Column<int>(type: "int", nullable: false),
                    NgayThamGia = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NhomMaNhom = table.Column<int>(type: "int", nullable: true),
                    NguoiDungMaNguoiDung = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThanhVienNhom", x => x.MaThanhVienNhom);
                    table.ForeignKey(
                        name: "FK_ThanhVienNhom_NguoiDung_NguoiDungMaNguoiDung",
                        column: x => x.NguoiDungMaNguoiDung,
                        principalTable: "NguoiDung",
                        principalColumn: "MaNguoiDung");
                    table.ForeignKey(
                        name: "FK_ThanhVienNhom_Nhom_NhomMaNhom",
                        column: x => x.NhomMaNhom,
                        principalTable: "Nhom",
                        principalColumn: "MaNhom");
                });

            migrationBuilder.CreateTable(
                name: "KetQuaBaiKiemTra",
                columns: table => new
                {
                    MaKetQua = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaBaiKiemTra = table.Column<int>(type: "int", nullable: false),
                    MaNguoiDung = table.Column<int>(type: "int", nullable: false),
                    DiemSo = table.Column<float>(type: "real", nullable: false),
                    NgayNop = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BaiKiemTraMaBaiKiemTra = table.Column<int>(type: "int", nullable: true),
                    NguoiDungMaNguoiDung = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KetQuaBaiKiemTra", x => x.MaKetQua);
                    table.ForeignKey(
                        name: "FK_KetQuaBaiKiemTra_BaiKiemTra_BaiKiemTraMaBaiKiemTra",
                        column: x => x.BaiKiemTraMaBaiKiemTra,
                        principalTable: "BaiKiemTra",
                        principalColumn: "MaBaiKiemTra");
                    table.ForeignKey(
                        name: "FK_KetQuaBaiKiemTra_NguoiDung_NguoiDungMaNguoiDung",
                        column: x => x.NguoiDungMaNguoiDung,
                        principalTable: "NguoiDung",
                        principalColumn: "MaNguoiDung");
                });

            migrationBuilder.CreateIndex(
                name: "IX_BaiHoc_KhoaHocMaKhoaHoc",
                table: "BaiHoc",
                column: "KhoaHocMaKhoaHoc");

            migrationBuilder.CreateIndex(
                name: "IX_BaiKiemTra_BaiHocMaBaiHoc",
                table: "BaiKiemTra",
                column: "BaiHocMaBaiHoc");

            migrationBuilder.CreateIndex(
                name: "IX_DangKyKhoaHoc_KhoaHocMaKhoaHoc",
                table: "DangKyKhoaHoc",
                column: "KhoaHocMaKhoaHoc");

            migrationBuilder.CreateIndex(
                name: "IX_DangKyKhoaHoc_NguoiDungMaNguoiDung",
                table: "DangKyKhoaHoc",
                column: "NguoiDungMaNguoiDung");

            migrationBuilder.CreateIndex(
                name: "IX_DichBaiHoc_BaiHocMaBaiHoc",
                table: "DichBaiHoc",
                column: "BaiHocMaBaiHoc");

            migrationBuilder.CreateIndex(
                name: "IX_DichBaiHoc_NgonNguMaNgonNgu",
                table: "DichBaiHoc",
                column: "NgonNguMaNgonNgu");

            migrationBuilder.CreateIndex(
                name: "IX_DichKhoaHoc_KhoaHocMaKhoaHoc",
                table: "DichKhoaHoc",
                column: "KhoaHocMaKhoaHoc");

            migrationBuilder.CreateIndex(
                name: "IX_DichKhoaHoc_NgonNguMaNgonNgu",
                table: "DichKhoaHoc",
                column: "NgonNguMaNgonNgu");

            migrationBuilder.CreateIndex(
                name: "IX_KetQuaBaiKiemTra_BaiKiemTraMaBaiKiemTra",
                table: "KetQuaBaiKiemTra",
                column: "BaiKiemTraMaBaiKiemTra");

            migrationBuilder.CreateIndex(
                name: "IX_KetQuaBaiKiemTra_NguoiDungMaNguoiDung",
                table: "KetQuaBaiKiemTra",
                column: "NguoiDungMaNguoiDung");

            migrationBuilder.CreateIndex(
                name: "IX_KhoaHoc_DanhMucMaDanhMuc",
                table: "KhoaHoc",
                column: "DanhMucMaDanhMuc");

            migrationBuilder.CreateIndex(
                name: "IX_KhoaHoc_GiaoVienMaNguoiDung",
                table: "KhoaHoc",
                column: "GiaoVienMaNguoiDung");

            migrationBuilder.CreateIndex(
                name: "IX_LichSuDangNhap_NguoiDungMaNguoiDung",
                table: "LichSuDangNhap",
                column: "NguoiDungMaNguoiDung");

            migrationBuilder.CreateIndex(
                name: "IX_LichTrinh_KhoaHocMaKhoaHoc",
                table: "LichTrinh",
                column: "KhoaHocMaKhoaHoc");

            migrationBuilder.CreateIndex(
                name: "IX_NhatKyHoatDong_NguoiDungMaNguoiDung",
                table: "NhatKyHoatDong",
                column: "NguoiDungMaNguoiDung");

            migrationBuilder.CreateIndex(
                name: "IX_Nhom_KhoaHocMaKhoaHoc",
                table: "Nhom",
                column: "KhoaHocMaKhoaHoc");

            migrationBuilder.CreateIndex(
                name: "IX_ThanhVienNhom_NguoiDungMaNguoiDung",
                table: "ThanhVienNhom",
                column: "NguoiDungMaNguoiDung");

            migrationBuilder.CreateIndex(
                name: "IX_ThanhVienNhom_NhomMaNhom",
                table: "ThanhVienNhom",
                column: "NhomMaNhom");

            migrationBuilder.CreateIndex(
                name: "IX_ThongBao_NguoiDungMaNguoiDung",
                table: "ThongBao",
                column: "NguoiDungMaNguoiDung");

            migrationBuilder.CreateIndex(
                name: "IX_TuongTac_BaiHocMaBaiHoc",
                table: "TuongTac",
                column: "BaiHocMaBaiHoc");

            migrationBuilder.CreateIndex(
                name: "IX_TuongTac_KhoaHocMaKhoaHoc",
                table: "TuongTac",
                column: "KhoaHocMaKhoaHoc");

            migrationBuilder.CreateIndex(
                name: "IX_TuongTac_NguoiDungMaNguoiDung",
                table: "TuongTac",
                column: "NguoiDungMaNguoiDung");

            migrationBuilder.CreateIndex(
                name: "IX_XepHang_KhoaHocMaKhoaHoc",
                table: "XepHang",
                column: "KhoaHocMaKhoaHoc");

            migrationBuilder.CreateIndex(
                name: "IX_XepHang_NguoiDungMaNguoiDung",
                table: "XepHang",
                column: "NguoiDungMaNguoiDung");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DangKyKhoaHoc");

            migrationBuilder.DropTable(
                name: "DichBaiHoc");

            migrationBuilder.DropTable(
                name: "DichKhoaHoc");

            migrationBuilder.DropTable(
                name: "KetQuaBaiKiemTra");

            migrationBuilder.DropTable(
                name: "LichSuDangNhap");

            migrationBuilder.DropTable(
                name: "LichTrinh");

            migrationBuilder.DropTable(
                name: "NhatKyHoatDong");

            migrationBuilder.DropTable(
                name: "ThanhVienNhom");

            migrationBuilder.DropTable(
                name: "ThongBao");

            migrationBuilder.DropTable(
                name: "TuongTac");

            migrationBuilder.DropTable(
                name: "XepHang");

            migrationBuilder.DropTable(
                name: "NgonNgu");

            migrationBuilder.DropTable(
                name: "BaiKiemTra");

            migrationBuilder.DropTable(
                name: "Nhom");

            migrationBuilder.DropTable(
                name: "BaiHoc");

            migrationBuilder.DropTable(
                name: "KhoaHoc");

            migrationBuilder.DropTable(
                name: "DanhMuc");

            migrationBuilder.DropTable(
                name: "NguoiDung");
        }
    }
}
