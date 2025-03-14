using System.ComponentModel.DataAnnotations;

namespace LearningManagementSystem.Models
{
    public class DichKhoaHoc
    {
        [Key]
        public int MaDich { get; set; }

        [Required]
        public int MaKhoaHoc { get; set; }

        [Required]
        public int MaNgonNgu { get; set; }

        [Required, StringLength(200)]
        public string TenKhoaHoc { get; set; }

        public string MoTa { get; set; }

        // Navigation properties
        public KhoaHoc? KhoaHoc { get; set; }
        public NgonNgu? NgonNgu { get; set; }
    }
}
