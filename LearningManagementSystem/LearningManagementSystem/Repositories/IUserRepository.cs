using LearningManagementSystem.Models;

namespace LearningManagementSystem.Repositories
{
    public interface IUserRepository
    {
        User GetById(string userId);
        IEnumerable<User> GetAll();
        void Add(User user);
        void Update(User user);
        void Delete(User user);
    }
}
