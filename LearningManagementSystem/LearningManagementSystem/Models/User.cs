using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace LearningManagementSystem.Models
{
    public class User
    {
        [Key]
        [Required , StringLength(50)]
        public string UserId { get; set; }

        [Required, StringLength(50)]
        public string UserName { get; set; }

        [Required, StringLength(100)]
        public string Password { get; set; }

        [StringLength(100)]
        public string FullName { get; set; }

        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(50)]
        public string RoleId { get; set; }

        public Role Roles { get; set; }

        public List<Comment> Comments { get; set; } = new List<Comment>();

        public List<Enrollment> Enrollments { get; set; }
        public List<Course> Courses { get; set; } 
        public List<Progress> Progresses { get; set; }
    }
}
