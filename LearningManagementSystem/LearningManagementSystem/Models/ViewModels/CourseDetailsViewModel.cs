namespace LearningManagementSystem.Models.ViewModels
{
    public class CourseDetailsViewModel
    {

        public Course Course { get; set; }
        public bool IsEnrolled { get; set; }
        public List<Comment> Comments { get; set; }
    }
}
