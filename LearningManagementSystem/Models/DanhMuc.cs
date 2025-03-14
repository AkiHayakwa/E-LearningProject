using System.ComponentModel.DataAnnotations;

namespace LearningManagementSystem.Models
{
    public class DanhMuc
    {
        [Key]
        public int MaDanhMuc { get; set; }

        [Required, StringLength(100)]
        public string TenDanhMuc { get; set; }

        public string MoTa { get; set; }

        // Navigation properties
        public List<KhoaHoc>? KhoaHoc { get; set; }
    }
}
