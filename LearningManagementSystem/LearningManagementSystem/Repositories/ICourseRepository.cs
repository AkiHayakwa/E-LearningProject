using LearningManagementSystem.Models;

namespace LearningManagementSystem.Repositories
{
    public interface ICourseRepository
    {
        IEnumerable<Course> GetAll();
        Course GetById(string courseId);
        IEnumerable<Course> GetCoursesByInstructor(string instructorId);
        void Add(Course course);
        void Update(Course course);
        void Delete(string courseId);
        void Save();
    }
}
