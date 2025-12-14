using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using LearningManagementSystem.Models;

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

        // DbSet
        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Progress> Progresses { get; set; }
        public DbSet<CourseInstructor> CourseInstructors { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<AssignmentQuestion> AssignmentQuestions { get; set; }
        public DbSet<AssignmentQuestionOption> AssignmentQuestionOptions { get; set; }
        public DbSet<AssignmentSubmission> AssignmentSubmissions { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<RevenueShare> RevenueShares { get; set; }
        public DbSet<WithdrawalRequest> WithdrawalRequests { get; set; }

        public DbSet<AIPractice> AIPractices { get; set; }
        public DbSet<StudySession> StudySessions { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<CourseTag> CourseTags { get; set; }

        // Forum
        public DbSet<Forum> Forums { get; set; }
        public DbSet<Topic> Topics { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<PostLike> PostLikes { get; set; }
        public DbSet<PostAttachment> PostAttachments { get; set; }
        public DbSet<TopicTag> TopicTags { get; set; }
        public DbSet<Report> Reports { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ===================== Role =====================
            modelBuilder.Entity<Role>()
                .HasKey(r => r.RoleId)
                .HasName("PK_Roles");
            modelBuilder.Entity<Role>()
                .Property(r => r.RoleId).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<Role>()
                .Property(r => r.RoleName).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<Role>()
                .HasMany(r => r.Users)
                .WithOne(u => u.Role)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // ===================== User =====================
            modelBuilder.Entity<User>()
                .HasKey(u => u.UserName)
                .HasName("PK_Users");
            modelBuilder.Entity<User>()
                .Property(u => u.UserName).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<User>()
                .Property(u => u.Password).HasMaxLength(256).IsRequired();
            modelBuilder.Entity<User>()
                .Property(u => u.FullName).HasMaxLength(100);
            modelBuilder.Entity<User>()
                .Property(u => u.Email).HasMaxLength(100).IsRequired();
            modelBuilder.Entity<User>()
                .Property(u => u.RoleId).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<User>()
                .Property(u => u.Avatar).HasMaxLength(200);
            modelBuilder.Entity<User>()
                .Property(u => u.Bio).HasMaxLength(1000);
            modelBuilder.Entity<User>()
                .Property(u => u.WalletBalance).HasColumnType("decimal(10,2)").HasDefaultValue(0);
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email).IsUnique()
                .HasDatabaseName("UQ_Users_Email");
            modelBuilder.Entity<User>()
                .HasMany(u => u.Carts)
                .WithOne(c => c.User)
                .HasForeignKey(c => c.UserName)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<User>()
                .HasMany(u => u.Enrollments)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserName)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<User>()
                .HasMany(u => u.Comments)
                .WithOne(c => c.User)
                .HasForeignKey(c => c.UserName)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<User>()
                .HasMany(u => u.Progresses)
                .WithOne(p => p.User)
                .HasForeignKey(p => p.UserName)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<User>()
                .HasMany(u => u.Payments)
                .WithOne(p => p.User)
                .HasForeignKey(p => p.UserName)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<User>()
                .HasMany(u => u.Notifications)
                .WithOne(n => n.User)
                .HasForeignKey(n => n.UserName)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<User>()
                .HasMany(u => u.CourseInstructors)
                .WithOne(ci => ci.User)
                .HasForeignKey(ci => ci.UserName)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<User>()
                .HasMany(u => u.AssignmentSubmissions)
                .WithOne(s => s.User)
                .HasForeignKey(s => s.UserName)
                .OnDelete(DeleteBehavior.Restrict);
            // ===================== Course =====================
            modelBuilder.Entity<Course>()
                .HasKey(c => c.CourseId)
                .HasName("PK_Courses");
            modelBuilder.Entity<Course>()
                .Property(c => c.CourseId).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<Course>()
                .Property(c => c.CourseName).HasMaxLength(100).IsRequired();
            modelBuilder.Entity<Course>()
                .Property(c => c.Description).HasMaxLength(1000).IsRequired();
            modelBuilder.Entity<Course>()
                .Property(c => c.CreatedDate).IsRequired();
            modelBuilder.Entity<Course>()
                .Property(c => c.ImageUrl).HasMaxLength(200);
            modelBuilder.Entity<Course>()
                .Property(c => c.Price).HasColumnType("decimal(10,2)");
            modelBuilder.Entity<Course>()
                .HasMany(c => c.Lessons)
                .WithOne(l => l.Course)
                .HasForeignKey(l => l.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Course>()
                .HasMany(c => c.Enrollments)
                .WithOne(e => e.Course)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Course>()
                .HasMany(c => c.Comments)
                .WithOne(c => c.Course)
                .HasForeignKey(c => c.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Course>()
                .HasMany(c => c.CourseInstructors)
                .WithOne(ci => ci.Course)
                .HasForeignKey(ci => ci.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Course>()
                .HasMany(c => c.Payments)
                .WithOne(p => p.Course)
                .HasForeignKey(p => p.CourseId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Course>()
                .HasMany(c => c.Assignments)
                .WithOne(a => a.Course)
                .HasForeignKey(a => a.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Course>()
                .HasMany(c => c.CartItems)
                .WithOne(ci => ci.Course)
                .HasForeignKey(ci => ci.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Course>()
                .HasMany(c => c.OrderDetails)
                .WithOne(od => od.Course)
                .HasForeignKey(od => od.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Course>()
                .Property(c => c.Level)
                .HasMaxLength(50);

            modelBuilder.Entity<Course>()
                .Property(c => c.DurationMinutes);

            // ===================== Tag =====================
            modelBuilder.Entity<Tag>()
                .HasKey(t => t.TagId)
                .HasName("PK_Tags");
            modelBuilder.Entity<Tag>()
                .Property(t => t.TagId).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<Tag>()
                .Property(t => t.Name).HasMaxLength(100).IsRequired();
            modelBuilder.Entity<Tag>()
                .Property(t => t.Slug).HasMaxLength(120);
            modelBuilder.Entity<Tag>()
                .Property(t => t.Category).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<Tag>()
                .Property(t => t.CreatedBy).HasMaxLength(50);
            modelBuilder.Entity<Tag>()
                .HasIndex(t => t.Slug)
                .IsUnique()
                .HasDatabaseName("UQ_Tags_Slug");

            modelBuilder.Entity<CourseTag>()
                .HasKey(ct => new { ct.CourseId, ct.TagId })
                .HasName("PK_CourseTags");
            modelBuilder.Entity<CourseTag>()
                .HasOne(ct => ct.Course)
                .WithMany(c => c.CourseTags)
                .HasForeignKey(ct => ct.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<CourseTag>()
                .HasOne(ct => ct.Tag)
                .WithMany(t => t.CourseTags)
                .HasForeignKey(ct => ct.TagId)
                .OnDelete(DeleteBehavior.Cascade);

            // ===================== Lesson =====================
            modelBuilder.Entity<Lesson>()
                .HasKey(l => l.LessonId)
                .HasName("PK_Lessons");
            modelBuilder.Entity<Lesson>()
                .Property(l => l.LessonId).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<Lesson>()
                .Property(l => l.CourseId).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<Lesson>()
                .Property(l => l.LessonTitle).HasMaxLength(100).IsRequired();
            modelBuilder.Entity<Lesson>()
                .Property(l => l.Content).HasMaxLength(4000);
            modelBuilder.Entity<Lesson>()
                .Property(l => l.VideoUrl).HasMaxLength(200);
            modelBuilder.Entity<Lesson>()
                .Property(l => l.OrderNumber).IsRequired();
            modelBuilder.Entity<Lesson>()
                .HasIndex(l => l.CourseId)
                .HasDatabaseName("IX_Lessons_CourseId");
            modelBuilder.Entity<Lesson>()
                .HasMany(l => l.Progresses)
                .WithOne(p => p.Lesson)
                .HasForeignKey(p => p.LessonId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Lesson>()
                .HasMany(l => l.Assignments)
                .WithOne(a => a.Lesson)
                .HasForeignKey(a => a.LessonId)
                .OnDelete(DeleteBehavior.Restrict); // Changed to Restrict to avoid cascade conflict

            // ===================== Enrollment =====================
            modelBuilder.Entity<Enrollment>()
                .HasKey(e => e.EnrollmentId)
                .HasName("PK_Enrollments");
            modelBuilder.Entity<Enrollment>()
                .Property(e => e.EnrollmentId).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<Enrollment>()
                .Property(e => e.UserName).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<Enrollment>()
                .Property(e => e.CourseId).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<Enrollment>()
                .Property(e => e.EnrollmentDate).IsRequired();
            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.User)
                .WithMany(u => u.Enrollments)
                .HasForeignKey(e => e.UserName)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Enrollment>()
                .HasIndex(e => new { e.UserName, e.CourseId }).IsUnique()
                .HasDatabaseName("UQ_Enrollments_UserName_CourseId");

            // ===================== Comment =====================
            modelBuilder.Entity<Comment>()
                .HasKey(c => c.CommentId)
                .HasName("PK_Comments");
            modelBuilder.Entity<Comment>()
                .Property(c => c.CommentId).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<Comment>()
                .Property(c => c.UserName).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<Comment>()
                .Property(c => c.CourseId).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<Comment>()
                .Property(c => c.Content).HasMaxLength(1000).IsRequired();
            modelBuilder.Entity<Comment>()
                .Property(c => c.CreatedDate).IsRequired();
            modelBuilder.Entity<Comment>()
                .Property(c => c.Rating).HasConversion<int?>();
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserName)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Course)
                .WithMany(c => c.Comments)
                .HasForeignKey(c => c.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Comment>()
                .HasIndex(c => new { c.UserName, c.CourseId })
                .HasDatabaseName("IX_Comments_UserName_CourseId");

            // ===================== Progress =====================
            modelBuilder.Entity<Progress>()
                .HasKey(p => p.ProgressId)
                .HasName("PK_Progresses");
            modelBuilder.Entity<Progress>()
                .Property(p => p.ProgressId).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<Progress>()
                .Property(p => p.UserName).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<Progress>()
                .Property(p => p.LessonId).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<Progress>()
                .Property(p => p.CompletionStatus).IsRequired();
            modelBuilder.Entity<Progress>()
                .Property(p => p.CompletionDate);
            modelBuilder.Entity<Progress>()
                .HasOne(p => p.User)
                .WithMany(u => u.Progresses)
                .HasForeignKey(p => p.UserName)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Progress>()
                .HasOne(p => p.Lesson)
                .WithMany(l => l.Progresses)
                .HasForeignKey(p => p.LessonId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Progress>()
                .HasIndex(p => new { p.UserName, p.LessonId }).IsUnique()
                .HasDatabaseName("UQ_Progresses_UserName_LessonId");

            // ===================== CourseInstructor =====================
            modelBuilder.Entity<CourseInstructor>()
                .HasKey(ci => new { ci.CourseId, ci.UserName })
                .HasName("PK_CourseInstructors");
            modelBuilder.Entity<CourseInstructor>()
                .Property(ci => ci.CourseId).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<CourseInstructor>()
                .Property(ci => ci.UserName).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<CourseInstructor>()
                .HasOne(ci => ci.Course)
                .WithMany(c => c.CourseInstructors)
                .HasForeignKey(ci => ci.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<CourseInstructor>()
                .HasOne(ci => ci.User)
                .WithMany(u => u.CourseInstructors)
                .HasForeignKey(ci => ci.UserName)
                .OnDelete(DeleteBehavior.Restrict);

            // ===================== Payment =====================
            modelBuilder.Entity<Payment>()
                .HasKey(p => p.PaymentId)
                .HasName("PK_Payments");
            modelBuilder.Entity<Payment>()
                .Property(p => p.PaymentId).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<Payment>()
                .Property(p => p.UserName).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<Payment>()
                .Property(p => p.CourseId).HasMaxLength(50);
            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount).HasColumnType("decimal(10,2)").IsRequired();
            modelBuilder.Entity<Payment>()
                .Property(p => p.PaymentDate).IsRequired();
            modelBuilder.Entity<Payment>()
                .Property(p => p.PaymentStatus).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<Payment>()
                .Property(p => p.PaymentMethod).HasMaxLength(50);
            modelBuilder.Entity<Payment>()
                .Property(p => p.TransactionType).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.User)
                .WithMany(u => u.Payments)
                .HasForeignKey(p => p.UserName)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Course)
                .WithMany(c => c.Payments)
                .HasForeignKey(p => p.CourseId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Payment>()
                .HasMany(p => p.OrderDetails)
                .WithOne(od => od.Payment)
                .HasForeignKey(od => od.PaymentId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Payment>()
                .HasIndex(p => new { p.UserName, p.CourseId })
                .HasDatabaseName("IX_Payments_UserName_CourseId");

            // ===================== Notification =====================
            modelBuilder.Entity<Notification>()
                .HasKey(n => n.NotificationId)
                .HasName("PK_Notifications");
            modelBuilder.Entity<Notification>()
                .Property(n => n.NotificationId).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<Notification>()
                .Property(n => n.UserName).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<Notification>()
                .Property(n => n.Title).HasMaxLength(255).IsRequired();
            modelBuilder.Entity<Notification>()
                .Property(n => n.Content).HasMaxLength(1000).IsRequired();
            modelBuilder.Entity<Notification>()
                .Property(n => n.CreatedDate).IsRequired();
            modelBuilder.Entity<Notification>()
                .Property(n => n.IsRead).IsRequired();
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserName)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Notification>()
                .HasIndex(n => n.UserName)
                .HasDatabaseName("IX_Notifications_UserName");

            // ===================== Assignment =====================
            modelBuilder.Entity<Assignment>()
                .HasKey(a => a.AssignmentId)
                .HasName("PK_Assignments");
            modelBuilder.Entity<Assignment>()
                .Property(a => a.AssignmentId).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<Assignment>()
                .Property(a => a.CourseId).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<Assignment>()
                .Property(a => a.LessonId).HasMaxLength(50).IsRequired(false);
            modelBuilder.Entity<Assignment>()
                .Property(a => a.AssignmentType).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<Assignment>()
                .Property(a => a.Title).HasMaxLength(255).IsRequired();
            modelBuilder.Entity<Assignment>()
                .Property(a => a.Description).HasMaxLength(1000);
            modelBuilder.Entity<Assignment>()
                .Property(a => a.DurationMinutes);
            modelBuilder.Entity<Assignment>()
                .Property(a => a.CreatedDate).IsRequired();
            modelBuilder.Entity<Assignment>()
                .HasOne(a => a.Course)
                .WithMany(c => c.Assignments)
                .HasForeignKey(a => a.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Assignment>()
                .HasOne(a => a.Lesson)
                .WithMany(l => l.Assignments)
                .HasForeignKey(a => a.LessonId)
                .OnDelete(DeleteBehavior.Restrict); // Changed to Restrict to avoid cascade conflict
            modelBuilder.Entity<Assignment>()
                .HasMany(a => a.Questions)
                .WithOne(q => q.Assignment)
                .HasForeignKey(q => q.AssignmentId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Assignment>()
                .HasMany(a => a.Submissions)
                .WithOne(s => s.Assignment)
                .HasForeignKey(s => s.AssignmentId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Assignment>()
                .HasIndex(a => a.CourseId)
                .HasDatabaseName("IX_Assignments_CourseId");
            modelBuilder.Entity<Assignment>()
                .HasIndex(a => a.LessonId)
                .HasDatabaseName("IX_Assignments_LessonId");

            // ===================== AssignmentQuestion =====================
            modelBuilder.Entity<AssignmentQuestion>()
       .HasKey(q => q.QuestionId)
       .HasName("PK_AssignmentQuestions");
            modelBuilder.Entity<AssignmentQuestion>()
                .Property(q => q.QuestionId).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<AssignmentQuestion>()
                .Property(q => q.AssignmentId).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<AssignmentQuestion>()
                .Property(q => q.QuestionType).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<AssignmentQuestion>()
                .Property(q => q.OrderNumber).IsRequired();
            modelBuilder.Entity<AssignmentQuestion>()
                .Property(q => q.MaxScore).HasColumnType("decimal(5,2)").IsRequired().HasDefaultValue(1.0);
            modelBuilder.Entity<AssignmentQuestion>()
                .HasOne(q => q.Assignment)
                .WithMany(a => a.Questions) // Hoàn chỉnh quan hệ với Assignment
                .HasForeignKey(q => q.AssignmentId) // Sử dụng AssignmentId làm khóa ngoại
                .OnDelete(DeleteBehavior.Cascade); // Cascade xóa khi Assignment bị xóa
            modelBuilder.Entity<AssignmentQuestion>()
                .HasMany(q => q.Options)
                .WithOne(o => o.Question)
                .HasForeignKey(o => o.QuestionId)
                .OnDelete(DeleteBehavior.Cascade); // Quan hệ với AssignmentQuestionOption
            modelBuilder.Entity<AssignmentQuestion>()
                .HasMany(q => q.Submissions)
                .WithOne(s => s.Question)
                .HasForeignKey(s => s.QuestionId)
                .OnDelete(DeleteBehavior.Restrict); // Quan hệ với AssignmentSubmission
            modelBuilder.Entity<AssignmentQuestion>()
                .HasIndex(q => q.AssignmentId)
                .HasDatabaseName("IX_AssignmentQuestions_AssignmentId");

            // ===================== AssignmentSubmission =====================
            modelBuilder.Entity<AssignmentSubmission>()
                .HasKey(s => s.SubmissionId)
                .HasName("PK_AssignmentSubmissions");
            modelBuilder.Entity<AssignmentSubmission>()
                .Property(s => s.SubmissionId).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<AssignmentSubmission>()
                .Property(s => s.AssignmentId).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<AssignmentSubmission>()
                .Property(s => s.QuestionId).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<AssignmentSubmission>()
                .Property(s => s.UserName).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<AssignmentSubmission>()
                .Property(s => s.IsCorrect);
            modelBuilder.Entity<AssignmentSubmission>()
                .Property(s => s.Score).HasColumnType("decimal(5,2)");
            modelBuilder.Entity<AssignmentSubmission>()
                .Property(s => s.Feedback).HasMaxLength(1000).IsRequired(false);
            modelBuilder.Entity<AssignmentSubmission>()
                .Property(s => s.SubmittedDate).IsRequired();
            modelBuilder.Entity<AssignmentSubmission>()
                .Property(s => s.SelectedOptionText).HasMaxLength(1000).IsRequired(false);
            modelBuilder.Entity<AssignmentSubmission>()
                .Property(s => s.SelectedOptionLabel).HasMaxLength(10).IsRequired(false);
            modelBuilder.Entity<AssignmentSubmission>()
                .HasOne(s => s.Assignment)
                .WithMany(a => a.Submissions)
                .HasForeignKey(s => s.AssignmentId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<AssignmentSubmission>()
                .HasOne(s => s.Question)
                .WithMany(q => q.Submissions)
                .HasForeignKey(s => s.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<AssignmentSubmission>()
                .HasOne(s => s.User)
                .WithMany(u => u.AssignmentSubmissions)
                .HasForeignKey(s => s.UserName)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<AssignmentSubmission>()
                .HasIndex(s => new { s.AssignmentId, s.QuestionId, s.UserName }).IsUnique()
                .HasDatabaseName("UQ_AssignmentSubmissions_AssignmentId_QuestionId_UserName");

            // ===================== Cart =====================
            modelBuilder.Entity<Cart>()
                .HasKey(c => c.CartId)
                .HasName("PK_Carts");
            modelBuilder.Entity<Cart>()
                .Property(c => c.CartId).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<Cart>()
                .Property(c => c.UserName).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<Cart>()
                .Property(c => c.CreatedDate).IsRequired();
            modelBuilder.Entity<Cart>()
                .Property(c => c.LastModifiedDate).IsRequired();
            modelBuilder.Entity<Cart>()
                .Property(c => c.TotalAmount).HasColumnType("decimal(10,2)").IsRequired();
            modelBuilder.Entity<Cart>()
                .Property(c => c.Status).HasMaxLength(20).IsRequired();
            modelBuilder.Entity<Cart>()
                .HasOne(c => c.User)
                .WithMany(u => u.Carts)
                .HasForeignKey(c => c.UserName)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Cart>()
                .HasIndex(c => c.UserName)
                .IsUnique()
                .HasDatabaseName("UQ_Carts_UserName");

            // ===================== CartItem =====================
            modelBuilder.Entity<CartItem>()
                .HasKey(ci => ci.CartItemId)
                .HasName("PK_CartItems");
            modelBuilder.Entity<CartItem>()
                .Property(ci => ci.CartItemId).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<CartItem>()
                .Property(ci => ci.CartId).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<CartItem>()
                .Property(ci => ci.CourseId).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<CartItem>()
                .Property(ci => ci.AddedDate).IsRequired();
            modelBuilder.Entity<CartItem>()
                .Property(ci => ci.Price).HasColumnType("decimal(10,2)").IsRequired();
            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Cart)
                .WithMany(c => c.CartItems)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Course)
                .WithMany(c => c.CartItems)
                .HasForeignKey(ci => ci.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<CartItem>()
                .HasIndex(ci => new { ci.CartId, ci.CourseId }).IsUnique()
                .HasDatabaseName("UQ_CartItems_CartId_CourseId");

            // ===================== OrderDetail =====================
            modelBuilder.Entity<OrderDetail>()
                .HasKey(od => od.OrderDetailId)
                .HasName("PK_OrderDetails");
            modelBuilder.Entity<OrderDetail>()
                .Property(od => od.OrderDetailId).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<OrderDetail>()
                .Property(od => od.PaymentId).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<OrderDetail>()
                .Property(od => od.CourseId).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<OrderDetail>()
                .Property(od => od.Price).HasColumnType("decimal(10,2)").IsRequired();
            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Payment)
                .WithMany(p => p.OrderDetails)
                .HasForeignKey(od => od.PaymentId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Course)
                .WithMany(c => c.OrderDetails)
                .HasForeignKey(od => od.CourseId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<OrderDetail>()
                .HasIndex(od => new { od.PaymentId, od.CourseId }).IsUnique()
                .HasDatabaseName("UQ_OrderDetails_PaymentId_CourseId");

            // ===================== RevenueShare =====================
            modelBuilder.Entity<RevenueShare>()
                .HasKey(rs => rs.RevenueShareId)
                .HasName("PK_RevenueShares");
            modelBuilder.Entity<RevenueShare>()
                .Property(rs => rs.RevenueShareId).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<RevenueShare>()
                .Property(rs => rs.PaymentId).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<RevenueShare>()
                .Property(rs => rs.UserName).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<RevenueShare>()
                .Property(rs => rs.CourseId).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<RevenueShare>()
                .Property(rs => rs.Amount).HasColumnType("decimal(10,2)").IsRequired();
            modelBuilder.Entity<RevenueShare>()
                .Property(rs => rs.Percentage).HasColumnType("decimal(5,2)").IsRequired();
            modelBuilder.Entity<RevenueShare>()
                .Property(rs => rs.ShareType).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<RevenueShare>()
                .Property(rs => rs.CreatedDate).IsRequired();
            modelBuilder.Entity<RevenueShare>()
                .HasOne(rs => rs.Payment)
                .WithMany()
                .HasForeignKey(rs => rs.PaymentId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<RevenueShare>()
                .HasOne(rs => rs.User)
                .WithMany()
                .HasForeignKey(rs => rs.UserName)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<RevenueShare>()
                .HasOne(rs => rs.Course)
                .WithMany()
                .HasForeignKey(rs => rs.CourseId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<RevenueShare>()
                .HasIndex(rs => rs.PaymentId)
                .HasDatabaseName("IX_RevenueShares_PaymentId");
            modelBuilder.Entity<RevenueShare>()
                .HasIndex(rs => rs.UserName)
                .HasDatabaseName("IX_RevenueShares_UserName");

            // ===================== WithdrawalRequest =====================
            modelBuilder.Entity<WithdrawalRequest>()
                .HasKey(wr => wr.WithdrawalRequestId)
                .HasName("PK_WithdrawalRequests");
            modelBuilder.Entity<WithdrawalRequest>()
                .Property(wr => wr.WithdrawalRequestId).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<WithdrawalRequest>()
                .Property(wr => wr.UserName).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<WithdrawalRequest>()
                .Property(wr => wr.Amount).HasColumnType("decimal(10,2)").IsRequired();
            modelBuilder.Entity<WithdrawalRequest>()
                .Property(wr => wr.BankName).HasMaxLength(100).IsRequired();
            modelBuilder.Entity<WithdrawalRequest>()
                .Property(wr => wr.AccountNumber).HasMaxLength(20).IsRequired();
            modelBuilder.Entity<WithdrawalRequest>()
                .Property(wr => wr.AccountHolderName).HasMaxLength(100);
            modelBuilder.Entity<WithdrawalRequest>()
                .Property(wr => wr.Status).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<WithdrawalRequest>()
                .Property(wr => wr.AdminNote).HasMaxLength(1000);
            modelBuilder.Entity<WithdrawalRequest>()
                .Property(wr => wr.RequestDate).IsRequired();
            modelBuilder.Entity<WithdrawalRequest>()
                .Property(wr => wr.ProcessedBy).HasMaxLength(50);
            modelBuilder.Entity<WithdrawalRequest>()
                .HasOne(wr => wr.User)
                .WithMany()
                .HasForeignKey(wr => wr.UserName)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<WithdrawalRequest>()
                .HasOne(wr => wr.ProcessedByUser)
                .WithMany()
                .HasForeignKey(wr => wr.ProcessedBy)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<WithdrawalRequest>()
                .HasIndex(wr => wr.UserName)
                .HasDatabaseName("IX_WithdrawalRequests_UserName");
            modelBuilder.Entity<WithdrawalRequest>()
                .HasIndex(wr => wr.Status)
                .HasDatabaseName("IX_WithdrawalRequests_Status");

            // ===================== AIPractice (AI Tutor Practice) =====================
            modelBuilder.Entity<AIPractice>()
                .HasKey(ap => ap.Id)
                .HasName("PK_AIPractices");

            modelBuilder.Entity<AIPractice>()
                .Property(ap => ap.Id).HasMaxLength(50).IsRequired();

            modelBuilder.Entity<AIPractice>()
                .Property(ap => ap.UserName).HasMaxLength(50).IsRequired();

            modelBuilder.Entity<AIPractice>()
                .Property(ap => ap.CourseId).HasMaxLength(50).IsRequired();

            modelBuilder.Entity<AIPractice>()
                .Property(ap => ap.Question).HasMaxLength(1000).IsRequired();

            modelBuilder.Entity<AIPractice>()
                .Property(ap => ap.Options).HasMaxLength(2000).IsRequired();

            modelBuilder.Entity<AIPractice>()
                .Property(ap => ap.CorrectAnswer).HasMaxLength(10).IsRequired();

            modelBuilder.Entity<AIPractice>()
                .Property(ap => ap.UserAnswer).HasMaxLength(10);

            modelBuilder.Entity<AIPractice>()
                .Property(ap => ap.CreatedAt).IsRequired();

            modelBuilder.Entity<AIPractice>()
                .HasOne(ap => ap.User)
                .WithMany(u => u.AIPractices)
                .HasForeignKey(ap => ap.UserName)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AIPractice>()
                .HasOne(ap => ap.Course)
                .WithMany(c => c.AIPractices)
                .HasForeignKey(ap => ap.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AIPractice>()
                .HasIndex(ap => new { ap.UserName, ap.CourseId })
                .HasDatabaseName("IX_AIPractices_UserName_CourseId");

            // ===================== StudySession =====================
            modelBuilder.Entity<StudySession>()
                .HasKey(ss => ss.SessionId)
                .HasName("PK_StudySessions");

            modelBuilder.Entity<StudySession>()
                .Property(ss => ss.SessionId).HasMaxLength(50).IsRequired();

            modelBuilder.Entity<StudySession>()
                .Property(ss => ss.UserName).HasMaxLength(50).IsRequired();

            modelBuilder.Entity<StudySession>()
                .Property(ss => ss.CourseId).HasMaxLength(50).IsRequired();

            modelBuilder.Entity<StudySession>()
                .Property(ss => ss.LessonId).HasMaxLength(50);

            modelBuilder.Entity<StudySession>()
                .Property(ss => ss.DurationSeconds).IsRequired();

            modelBuilder.Entity<StudySession>()
                .Property(ss => ss.Note).HasMaxLength(200);

            modelBuilder.Entity<StudySession>()
                .HasOne(ss => ss.User)
                .WithMany(u => u.StudySessions)
                .HasForeignKey(ss => ss.UserName)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StudySession>()
                .HasOne(ss => ss.Course)
                .WithMany(c => c.StudySessions)
                .HasForeignKey(ss => ss.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StudySession>()
                .HasOne(ss => ss.Lesson)
                .WithMany(l => l.StudySessions)
                .HasForeignKey(ss => ss.LessonId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StudySession>()
                .HasIndex(ss => new { ss.UserName, ss.CourseId, ss.StartedAt })
                .HasDatabaseName("IX_StudySessions_User_Course_StartedAt");

            // ===================== Forum =====================
            modelBuilder.Entity<Forum>()
                .HasKey(f => f.ForumId)
                .HasName("PK_Forums");
            modelBuilder.Entity<Forum>()
                .Property(f => f.ForumId).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<Forum>()
                .Property(f => f.CourseId).HasMaxLength(50);
            modelBuilder.Entity<Forum>()
                .Property(f => f.Name).HasMaxLength(200).IsRequired();
            modelBuilder.Entity<Forum>()
                .Property(f => f.Description).HasMaxLength(1000);
            modelBuilder.Entity<Forum>()
                .HasOne(f => f.Course)
                .WithMany()
                .HasForeignKey(f => f.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Forum>()
                .HasMany(f => f.Topics)
                .WithOne(t => t.Forum)
                .HasForeignKey(t => t.ForumId)
                .OnDelete(DeleteBehavior.Cascade);

            // ===================== Topic =====================
            modelBuilder.Entity<Topic>()
                .HasKey(t => t.TopicId)
                .HasName("PK_Topics");
            modelBuilder.Entity<Topic>()
                .Property(t => t.TopicId).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<Topic>()
                .Property(t => t.ForumId).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<Topic>()
                .Property(t => t.UserName).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<Topic>()
                .Property(t => t.Title).HasMaxLength(200).IsRequired();
            modelBuilder.Entity<Topic>()
                .Property(t => t.Content).HasMaxLength(10000).IsRequired();
            modelBuilder.Entity<Topic>()
                .Property(t => t.Status).HasMaxLength(20).IsRequired();
            modelBuilder.Entity<Topic>()
                .Property(t => t.LastReplyBy).HasMaxLength(50);
            modelBuilder.Entity<Topic>()
                .HasOne(t => t.Forum)
                .WithMany(f => f.Topics)
                .HasForeignKey(t => t.ForumId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Topic>()
                .HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserName)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Topic>()
                .HasMany(t => t.Posts)
                .WithOne(p => p.Topic)
                .HasForeignKey(p => p.TopicId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Topic>()
                .HasIndex(t => t.ForumId)
                .HasDatabaseName("IX_Topics_ForumId");

            // ===================== Post =====================
            modelBuilder.Entity<Post>()
                .HasKey(p => p.PostId)
                .HasName("PK_Posts");
            modelBuilder.Entity<Post>()
                .Property(p => p.PostId).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<Post>()
                .Property(p => p.TopicId).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<Post>()
                .Property(p => p.UserName).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<Post>()
                .Property(p => p.Content).HasMaxLength(10000).IsRequired();
            modelBuilder.Entity<Post>()
                .Property(p => p.ParentPostId).HasMaxLength(50);
            modelBuilder.Entity<Post>()
                .HasOne(p => p.Topic)
                .WithMany(t => t.Posts)
                .HasForeignKey(p => p.TopicId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Post>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserName)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Post>()
                .HasOne(p => p.ParentPost)
                .WithMany(p => p.Replies)
                .HasForeignKey(p => p.ParentPostId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Post>()
                .HasMany(p => p.PostLikes)
                .WithOne(pl => pl.Post)
                .HasForeignKey(pl => pl.PostId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Post>()
                .HasMany(p => p.PostAttachments)
                .WithOne(pa => pa.Post)
                .HasForeignKey(pa => pa.PostId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Post>()
                .HasIndex(p => p.TopicId)
                .HasDatabaseName("IX_Posts_TopicId");
            modelBuilder.Entity<Post>()
                .HasIndex(p => p.ParentPostId)
                .HasDatabaseName("IX_Posts_ParentPostId");

            // ===================== PostLike =====================
            modelBuilder.Entity<PostLike>()
                .HasKey(pl => pl.LikeId)
                .HasName("PK_PostLikes");
            modelBuilder.Entity<PostLike>()
                .Property(pl => pl.LikeId).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<PostLike>()
                .Property(pl => pl.PostId).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<PostLike>()
                .Property(pl => pl.UserName).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<PostLike>()
                .HasOne(pl => pl.Post)
                .WithMany(p => p.PostLikes)
                .HasForeignKey(pl => pl.PostId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<PostLike>()
                .HasOne(pl => pl.User)
                .WithMany()
                .HasForeignKey(pl => pl.UserName)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<PostLike>()
                .HasIndex(pl => new { pl.PostId, pl.UserName })
                .IsUnique()
                .HasDatabaseName("UQ_PostLikes_PostId_UserName");

            // ===================== PostAttachment =====================
            modelBuilder.Entity<PostAttachment>()
                .HasKey(pa => pa.AttachmentId)
                .HasName("PK_PostAttachments");
            modelBuilder.Entity<PostAttachment>()
                .Property(pa => pa.AttachmentId).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<PostAttachment>()
                .Property(pa => pa.PostId).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<PostAttachment>()
                .Property(pa => pa.FileName).HasMaxLength(255).IsRequired();
            modelBuilder.Entity<PostAttachment>()
                .Property(pa => pa.FilePath).HasMaxLength(500).IsRequired();
            modelBuilder.Entity<PostAttachment>()
                .Property(pa => pa.FileType).HasMaxLength(50);
            modelBuilder.Entity<PostAttachment>()
                .HasOne(pa => pa.Post)
                .WithMany(p => p.PostAttachments)
                .HasForeignKey(pa => pa.PostId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<PostAttachment>()
                .HasIndex(pa => pa.PostId)
                .HasDatabaseName("IX_PostAttachments_PostId");

            // ===================== TopicTag =====================
            modelBuilder.Entity<TopicTag>()
                .HasKey(tt => tt.TagId)
                .HasName("PK_TopicTags");
            modelBuilder.Entity<TopicTag>()
                .Property(tt => tt.TagId).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<TopicTag>()
                .Property(tt => tt.TopicId).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<TopicTag>()
                .Property(tt => tt.TagName).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<TopicTag>()
                .HasOne(tt => tt.Topic)
                .WithMany(t => t.TopicTags)
                .HasForeignKey(tt => tt.TopicId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<TopicTag>()
                .HasIndex(tt => tt.TopicId)
                .HasDatabaseName("IX_TopicTags_TopicId");

            // ===================== Report =====================
            modelBuilder.Entity<Report>()
                .HasKey(r => r.ReportId)
                .HasName("PK_Reports");
            modelBuilder.Entity<Report>()
                .Property(r => r.ReportId).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<Report>()
                .Property(r => r.PostId).HasMaxLength(50);
            modelBuilder.Entity<Report>()
                .Property(r => r.TopicId).HasMaxLength(50);
            modelBuilder.Entity<Report>()
                .Property(r => r.ReportedBy).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<Report>()
                .Property(r => r.Reason).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<Report>()
                .Property(r => r.Description).HasMaxLength(1000);
            modelBuilder.Entity<Report>()
                .Property(r => r.Status).HasMaxLength(20).IsRequired();
            modelBuilder.Entity<Report>()
                .Property(r => r.ResolvedBy).HasMaxLength(50);
            modelBuilder.Entity<Report>()
                .HasOne(r => r.Post)
                .WithMany()
                .HasForeignKey(r => r.PostId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Report>()
                .HasOne(r => r.Topic)
                .WithMany()
                .HasForeignKey(r => r.TopicId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Report>()
                .HasOne(r => r.ReportedByUser)
                .WithMany()
                .HasForeignKey(r => r.ReportedBy)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Report>()
                .HasOne(r => r.ResolvedByUser)
                .WithMany()
                .HasForeignKey(r => r.ResolvedBy)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Report>()
                .HasIndex(r => r.Status)
                .HasDatabaseName("IX_Reports_Status");

        }
    }
}