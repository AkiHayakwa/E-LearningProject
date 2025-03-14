using System.ComponentModel.DataAnnotations;

namespace LearningManagementSystem.Models
{
    public class LichTrinh
    {
        [Key]
        public int MaLichTrinh { get; set; }

        [Required]
        public int MaKhoaHoc { get; set; }

        [Required]
        public DateTime ThoiGianBatDau { get; set; }

        [Required]
        public DateTime ThoiGianKetThuc { get; set; }

        [StringLength(200)]
        public string DiaDiem { get; set; }

        // Navigation properties
        public KhoaHoc? KhoaHoc { get; set; }
    }
}
