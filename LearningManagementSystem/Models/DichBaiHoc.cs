using System.ComponentModel.DataAnnotations;

namespace LearningManagementSystem.Models
{
    public class DichBaiHoc
    {
        [Key]
        public int MaDich { get; set; }

        [Required]
        public int MaBaiHoc { get; set; }

        [Required]
        public int MaNgonNgu { get; set; }

        [Required, StringLength(200)]
        public string TenBaiHoc { get; set; }

        public string NoiDungVanBan { get; set; }

        // Navigation properties
        public BaiHoc? BaiHoc { get; set; }
        public NgonNgu? NgonNgu { get; set; }
    }
}
