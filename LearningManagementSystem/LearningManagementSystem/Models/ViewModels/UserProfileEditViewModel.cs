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
        public string Title { get; set; }
    }
}
