using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace LearningManagementSystem.Models
{
    public class User
    {
        [Key]
        [Required, StringLength(50)]
        public string UserId { get; set; }

        [Required, StringLength(50)]
        public string UserName { get; set; }

        [Required, StringLength(100)]
        private string _password; // Lưu mật khẩu đã băm

        public string Password
        {
            get => _password;
            set => _password = value;
        }

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

        // Phương thức để băm mật khẩu
        public void HashPassword(IPasswordHasher<User> passwordHasher, string plainPassword)
        {
            _password = passwordHasher.HashPassword(this, plainPassword);
        }

        // Phương thức để xác minh mật khẩu
        public bool VerifyPassword(IPasswordHasher<User> passwordHasher, string plainPassword)
        {
            var result = passwordHasher.VerifyHashedPassword(this, _password, plainPassword);
            return result == PasswordVerificationResult.Success;
        }
    }
}