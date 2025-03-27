namespace LearningManagementSystem.Models.ViewModels
{
    public class HomeViewModel
    {
        public User User { get; set; }
        public List<Course> Courses { get; set; }
        public string SearchQuery { get; set; }
    }
}
