using LearningManagementSystem.Data;
using LearningManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace LearningManagementSystem.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly LMSContext _context;

        // Constructor nhận DbContext qua DI
        public UserRepository(LMSContext context)
        {
            _context = context;
        }

        public User GetById(string userId)
        {
            return _context.Users.Include(u => u.Roles).FirstOrDefault(u => u.UserId == userId);
        }

        public IEnumerable<User> GetAll()
        {
            return _context.Users.Include(u => u.Roles).ToList();
        }

        public void Add(User user)
        {
            _context.Users.Add(user);
        }

        public void Update(User user)
        {
            _context.Users.Update(user);
        }

        public void Delete(User user)
        {
            _context.Users.Remove(user);
        }
    }
}
