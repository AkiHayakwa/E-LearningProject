namespace LearningManagementSystem.Models.ViewModels
{
    public class UserProfileEditViewModel
    {
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public List<CommentViewModel> Comments { get; set; }
        public List<CourseViewModel> EnrolledCourses { get; set; }
    }

    public class CommentViewModel
    {
        public string CommentId { get; set; }
        public string Content { get; set; }
        public DateTime CommentDate { get; set; }
        public string CourseTitle { get; set; }
    }

    public class CourseViewModel
    {
        public string CourseId { get; set; }
        public string Title { get; set; } // Đổi tên từ Title thành CourseName để đồng bộ với các ViewModel khác
        public string CourseName { get; set; } // Thêm thuộc tính này (có thể dùng Title thay thế, nhưng tôi thêm để rõ ràng)
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ImageUrl { get; set; }
        public bool IsEnrolled { get; set; } // Thêm thuộc tính để kiểm tra trạng thái đăng ký
        public List<Lesson> Lessons { get; set; }

    }
}