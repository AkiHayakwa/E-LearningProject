namespace LearningManagementSystem.Models.ViewModels
{
    public class CourseListViewModel
    {
        public string CourseId { get; set; }
        public string CourseName { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ImageUrl { get; set; }
        public bool IsEnrolled { get; set; }
    }
}
