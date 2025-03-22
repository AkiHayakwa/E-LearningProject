namespace LearningManagementSystem.Models.ViewModels
{
    public class HomeViewModel
    {
        public User User { get; set; }
        public List<Enrollment> Enrollments { get; set; }
        public List<Progress> Progresses { get; set; }
        public List<Comment> Comments { get; set; }
        public List<Comment> Notifications { get; set; }
    }
}
