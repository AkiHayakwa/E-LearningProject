using LearningManagementSystem.Models;

namespace LearningManagementSystem.Repositories
{
    public interface ITopicRepository
    {
        IQueryable<Topic> GetAll();
        Topic? GetById(string id);
        IQueryable<Topic> GetByForumId(string forumId);
        IQueryable<Topic> GetByUserName(string userName);
        void Add(Topic topic);
        void Update(Topic topic);
        void Delete(string id);
        void Save();
    }
}

