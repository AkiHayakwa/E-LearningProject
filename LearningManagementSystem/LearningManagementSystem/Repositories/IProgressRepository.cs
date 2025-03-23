using LearningManagementSystem.Models;

namespace LearningManagementSystem.Repositories
{
    public interface IProgressRepository
    {
        IEnumerable<Progress> GetProgressByUser(string userId);
        IEnumerable<Progress> GetProgressByUserAndCourse(string userId, string courseId);
        Progress GetById(string progressId);

        IEnumerable<Progress> GetAll();
        void Add(Progress progress);
        void Update(Progress progress);
        void Delete(Progress progress);
    }
}
