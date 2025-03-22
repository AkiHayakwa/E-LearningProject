using LearningManagementSystem.Models;

namespace LearningManagementSystem.Repositories
{
    public interface IUserRepository
    {
        User GetUserWithRole(string userId);
        IEnumerable<User> GetAll();
        User GetById(string id);
        void Add(User user, string plainPassword);
        void Delete(User user);
        User GetByUserName(string userName);
    }
}
