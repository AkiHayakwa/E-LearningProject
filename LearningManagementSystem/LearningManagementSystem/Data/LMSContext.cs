using LearningManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System;

namespace LearningManagementSystem.Data
{
    public class LMSContext : DbContext
    {
        private readonly IPasswordHasher<User> _passwordHasher;

        public LMSContext(DbContextOptions<LMSContext> options, IPasswordHasher<User> passwordHasher = null)
            : base(options)
        {
            _passwordHasher = passwordHasher ?? new PasswordHasher<User>();
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<Progress> Progresses { get; set; }
        public DbSet<Comment> Comments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.UserName);
                entity.Property(u => u.UserName).HasMaxLength(50).IsRequired();
                entity.Property(u => u.FullName).HasMaxLength(100); // Bỏ IsRequired() để đồng bộ với model
                entity.Property(u => u.Email).HasMaxLength(100).IsRequired();
                entity.Property(u => u.Password).HasMaxLength(256).IsRequired(); // Sửa từ PasswordHash thành Password
                entity.Property(u => u.RoleId).IsRequired();

                // Quan hệ User - Role
                entity.HasOne(u => u.Role)
                      .WithMany(r => r.Users)
                      .HasForeignKey(u => u.RoleId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Chỉ mục cho Email (đảm bảo duy nhất)
                entity.HasIndex(u => u.Email).IsUnique();
            });

            // Role
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(r => r.RoleId);
                entity.Property(r => r.RoleId).HasMaxLength(50).IsRequired();
                entity.Property(r => r.RoleName).HasMaxLength(50).IsRequired();
            });

            // Course
            modelBuilder.Entity<Course>(entity =>
            {
                entity.HasKey(c => c.CourseId);
                entity.Property(c => c.CourseId).HasMaxLength(50).IsRequired();
                entity.Property(c => c.CourseName).HasMaxLength(100).IsRequired();
                entity.Property(c => c.Description).HasMaxLength(1000).IsRequired();
                entity.Property(c => c.CreatedDate).IsRequired();
                entity.Property(c => c.ImageUrl).HasMaxLength(200); // Thêm cấu hình cho ImageUrl
            });

            // Enrollment
            modelBuilder.Entity<Enrollment>(entity =>
            {
                entity.HasKey(e => e.EnrollmentId);
                entity.Property(e => e.EnrollmentId).HasMaxLength(50).IsRequired();
                entity.Property(e => e.UserName).HasMaxLength(50).IsRequired();
                entity.Property(e => e.CourseId).HasMaxLength(50).IsRequired();
                entity.Property(e => e.EnrollmentDate).IsRequired();

                // Quan hệ Enrollment - User
                entity.HasOne(e => e.User)
                      .WithMany(u => u.Enrollments)
                      .HasForeignKey(e => e.UserName)
                      .OnDelete(DeleteBehavior.Restrict);

                // Quan hệ Enrollment - Course
                entity.HasOne(e => e.Course)
                      .WithMany(c => c.Enrollments)
                      .HasForeignKey(e => e.CourseId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Chỉ mục cho khóa ngoại
                entity.HasIndex(e => new { e.UserName, e.CourseId }).IsUnique();
            });

            // Lesson
            modelBuilder.Entity<Lesson>(entity =>
            {
                entity.HasKey(l => l.LessonId);
                entity.Property(l => l.LessonId).HasMaxLength(50).IsRequired();
                entity.Property(l => l.CourseId).HasMaxLength(50).IsRequired();
                entity.Property(l => l.LessonTitle).HasMaxLength(100).IsRequired();
                entity.Property(l => l.Content).HasMaxLength(4000);
                entity.Property(l => l.LinkYoutube).HasMaxLength(200);
                entity.Property(l => l.OrderNumber).IsRequired();

                // Quan hệ Lesson - Course
                entity.HasOne(l => l.Course)
                      .WithMany(c => c.Lessons)
                      .HasForeignKey(l => l.CourseId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Chỉ mục cho khóa ngoại
                entity.HasIndex(l => l.CourseId);
            });

            // Progress
            modelBuilder.Entity<Progress>(entity =>
            {
                entity.HasKey(p => p.ProgressId);
                entity.Property(p => p.ProgressId).HasMaxLength(50).IsRequired();
                entity.Property(p => p.UserName).HasMaxLength(50).IsRequired();
                entity.Property(p => p.LessonId).HasMaxLength(50).IsRequired();
                entity.Property(p => p.CompletionStatus).IsRequired();
                entity.Property(p => p.CompletionDate); // Nullable DateTime

                // Quan hệ Progress - User
                entity.HasOne(p => p.User)
                      .WithMany(u => u.Progresses)
                      .HasForeignKey(p => p.UserName)
                      .OnDelete(DeleteBehavior.Restrict);

                // Quan hệ Progress - Lesson
                entity.HasOne(p => p.Lesson)
                      .WithMany(l => l.Progresses)
                      .HasForeignKey(p => p.LessonId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Chỉ mục cho khóa ngoại
                entity.HasIndex(p => new { p.UserName, p.LessonId }).IsUnique();
            });

            // Comment
            modelBuilder.Entity<Comment>(entity =>
            {
                entity.HasKey(c => c.CommentId);
                entity.Property(c => c.CommentId).HasMaxLength(50).IsRequired();
                entity.Property(c => c.UserName).HasMaxLength(50).IsRequired();
                entity.Property(c => c.CourseId).HasMaxLength(50).IsRequired();
                entity.Property(c => c.Content).HasMaxLength(1000).IsRequired();
                entity.Property(c => c.CreatedDate).IsRequired();

                // Quan hệ Comment - Course
                entity.HasOne(c => c.Course)
                      .WithMany(c => c.Comments)
                      .HasForeignKey(c => c.CourseId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Quan hệ Comment - User
                entity.HasOne(c => c.User)
                      .WithMany(u => u.Comments)
                      .HasForeignKey(c => c.UserName)
                      .OnDelete(DeleteBehavior.Restrict);

                // Chỉ mục cho khóa ngoại
                entity.HasIndex(c => new { c.UserName, c.CourseId });
            });

            // Dữ liệu khởi tạo (seeding)
            modelBuilder.Entity<Role>().HasData(
                new Role { RoleId = "role1", RoleName = "Admin" },
                new Role { RoleId = "role2", RoleName = "Student" }
            );

            // Tạo dữ liệu người dùng với Password đã băm
            var adminUser = new User
            {
                UserName = "admin1",
                FullName = "Admin One",
                Email = "admin1@example.com",
                Password = "", // Sẽ được cập nhật bên dưới
                RoleId = "role1"
            };
            adminUser.HashPassword(_passwordHasher, "Admin@123");

            var studentUser = new User
            {
                UserName = "student1",
                FullName = "Student One",
                Email = "student1@example.com",
                Password = "", // Sẽ được cập nhật bên dưới
                RoleId = "role2"
            };
            studentUser.HashPassword(_passwordHasher, "Student@123");

            modelBuilder.Entity<User>().HasData(adminUser, studentUser);

            modelBuilder.Entity<Course>().HasData(
                new Course
                {
                    CourseId = "course1",
                    CourseName = "Khóa học lập trình C# cơ bản",
                    Description = "Khóa học này giới thiệu các khái niệm cơ bản về lập trình C#.",
                    CreatedDate = DateTime.Now,
                    ImageUrl = "/images/course1.jpg" // Thêm ImageUrl
                },
                new Course
                {
                    CourseId = "course2",
                    CourseName = "Khóa học ASP.NET Core",
                    Description = "Khóa học này hướng dẫn xây dựng ứng dụng web với ASP.NET Core.",
                    CreatedDate = DateTime.Now,
                    ImageUrl = "/images/course2.jpg" // Thêm ImageUrl
                }
            );

            modelBuilder.Entity<Lesson>().HasData(
                new Lesson
                {
                    LessonId = "lesson1",
                    CourseId = "course1",
                    LessonTitle = "Giới thiệu về C#",
                    Content = "Bài học này giới thiệu về ngôn ngữ lập trình C#.",
                    LinkYoutube = "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
                    OrderNumber = 1
                },
                new Lesson
                {
                    LessonId = "lesson2",
                    CourseId = "course1",
                    LessonTitle = "Biến và kiểu dữ liệu",
                    Content = "Bài học này giải thích về biến và kiểu dữ liệu trong C#.",
                    LinkYoutube = "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
                    OrderNumber = 2
                }
            );

            modelBuilder.Entity<Enrollment>().HasData(
                new Enrollment
                {
                    EnrollmentId = "enrollment1",
                    UserName = "student1",
                    CourseId = "course1",
                    EnrollmentDate = DateTime.Now
                }
            );

            modelBuilder.Entity<Comment>().HasData(
                new Comment
                {
                    CommentId = "comment1",
                    UserName = "student1",
                    CourseId = "course1",
                    Content = "Khóa học rất hữu ích!",
                    CreatedDate = DateTime.Now
                }
            );

            modelBuilder.Entity<Progress>().HasData(
                new Progress
                {
                    ProgressId = "progress1",
                    UserName = "student1",
                    LessonId = "lesson1",
                    CompletionStatus = true,
                    CompletionDate = DateTime.Now
                }
            );
        }
    }
}