namespace LearningManagementSystem.Models.ViewModels
{
    public class CourseDetailsViewModel
    {
        public Course Course { get; set; }
        public bool IsEnrolled { get; set; }
        public List<Comment> Comments { get; set; }
        public List<Progress> Progresses { get; set; } // Thêm danh sách Progress

        public CourseDetailsViewModel()
        {
            Comments = new List<Comment>();
            Progresses = new List<Progress>();
        }
    }
}