using LearningManagementSystem.Models;

namespace LearningManagementSystem.Repositories
{
    public interface IForumRepository
    {
        IQueryable<Forum> GetAll();
        Forum? GetById(string id);
        Forum? GetByCourseId(string courseId);
        void Add(Forum forum);
        void Update(Forum forum);
        void Delete(string id);
        void Save();
    }
}

