using LearningManagementSystem.Models;

namespace LearningManagementSystem.Repositories
{
    public interface IUserRepository
    {
        IQueryable<User> GetAll();
        User GetByUserName(string userName);
        void Add(User user);
        void Update(User user);
        void Delete(string userName);
        void Save();
    }
}
