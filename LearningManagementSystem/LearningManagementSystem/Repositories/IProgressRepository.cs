using LearningManagementSystem.Models;

namespace LearningManagementSystem.Repositories
{
    public interface IProgressRepository
    {
        IEnumerable<Progress> GetProgressByUser(string userId);
        IEnumerable<Progress> GetProgressByUserAndCourse(string userId, string courseId); // Thêm phương thức mới
        Progress GetById(string progressId);
        void Add(Progress progress);
        void Update(Progress progress);
        void Delete(Progress progress);
    }
}
