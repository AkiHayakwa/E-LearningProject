namespace LearningManagementSystem.Models.ViewModels
{
    public class EnrollCourseViewModel
    {
        public Course Course { get; set; }
        public List<Lesson> Lessons { get; set; }
        public Lesson SelectedLesson { get; set; }
        public List<Comment> Comments { get; set; }
        public User Instructor { get; set; }
    }
}
