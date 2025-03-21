using LearningManagementSystem.Models;

namespace LearningManagementSystem.Repositories
{
    public interface ICourseRepository
    {
        Course GetById(string courseId);
        IEnumerable<Course> GetAll();
        void Add(Course course);
        void Update(Course course);
        void Delete(Course course);
    }
}
