using System.ComponentModel.DataAnnotations;

namespace LearningManagementSystem.Models
{
    public class Role
    {
        [Key]
        [StringLength(50)]
        public string RoleId { get; set; }

        [Required]
        [StringLength(20)]
        public string RoleName { get; set; }

        public List<User> Users { get; set; }
    }
}
