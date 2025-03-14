using System.ComponentModel.DataAnnotations;

namespace LearningManagementSystem.Models
{
    public class BaiKiemTra
    {
        [Key]
        public int MaBaiKiemTra { get; set; }

        [Required]
        public int MaBaiHoc { get; set; }

        [Required, StringLength(200)]
        public string TenBaiKiemTra { get; set; }

        [Required, Range(0, double.MaxValue)]
        public float DiemToiDa { get; set; }

        // Navigation properties
        public BaiHoc? BaiHoc { get; set; }
        public List<KetQuaBaiKiemTra>? KetQuaBaiKiemTra { get; set; }
    }
}
