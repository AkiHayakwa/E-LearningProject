using System.ComponentModel.DataAnnotations;

namespace LearningManagementSystem.Models
{
    public class NgonNgu
    {
        [Key]
        public int MaNgonNgu { get; set; }

        [Required, StringLength(10)]
        public string MaCodeNgonNgu { get; set; }

        [Required, StringLength(50)]
        public string TenNgonNgu { get; set; }

        // Navigation properties
        public List<DichKhoaHoc>? DichKhoaHoc { get; set; }
        public List<DichBaiHoc>? DichBaiHoc { get; set; }
    }
}
