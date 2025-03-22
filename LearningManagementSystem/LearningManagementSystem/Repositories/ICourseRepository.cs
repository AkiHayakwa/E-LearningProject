using LearningManagementSystem.Models;

namespace LearningManagementSystem.Repositories
{
    public interface ICourseRepository
    {
        Course GetCourseWithInstructor(string courseId);
        IEnumerable<Course> GetAllWithInstructor();
        IEnumerable<Course> GetAll();
        Course GetById(string id);
        void Add(Course course);
        void Delete(Course course);
    }
}
