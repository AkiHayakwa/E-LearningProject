namespace LearningManagementSystem.Models.ViewModels
{
    public class HomeViewModel
    {
        public User User { get; set; }
        public List<CourseViewModel> Courses { get; set; }
        public string SearchQuery { get; set; }
    }

}
