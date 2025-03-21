using LearningManagementSystem.Models;

namespace LearningManagementSystem.Repositories
{
    public interface IProgressRepository
    {
        IEnumerable<Progress> GetProgressByUserAndCourse(string userId, string courseId);
        void Add(Progress progress);
        void Update(Progress progress);
    }
}
