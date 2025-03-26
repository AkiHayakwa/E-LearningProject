using LearningManagementSystem.Models;

namespace LearningManagementSystem.Repositories
{
    public interface ICourseRepository
    {
        IQueryable<Course> GetAll();
        Course GetById(string courseId);
        void Add(Course course);
        void Update(Course course);
        void Delete(string courseId);
        void Save();
    }
}
