using LearningManagementSystem.Models;

namespace LearningManagementSystem.Repositories
{
    public interface IPostRepository
    {
        IQueryable<Post> GetAll();
        Post? GetById(string id);
        IQueryable<Post> GetByTopicId(string topicId);
        IQueryable<Post> GetRepliesByPostId(string postId);
        void Add(Post post);
        void Update(Post post);
        void Delete(string id);
        void Save();
    }
}

