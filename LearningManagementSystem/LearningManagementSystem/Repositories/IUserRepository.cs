using LearningManagementSystem.Models;

namespace LearningManagementSystem.Repositories
{
    public interface IUserRepository
    {
        IEnumerable<User> GetAll();
        User GetById(string userId);
        void Add(User user);
        void Update(User user);
        void Delete(string userId);
        void Save();
    }
}
