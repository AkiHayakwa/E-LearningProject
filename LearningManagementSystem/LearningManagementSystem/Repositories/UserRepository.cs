using LearningManagementSystem.Data;
using LearningManagementSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LearningManagementSystem.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly LMSContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        public UserRepository(LMSContext context, IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public IEnumerable<User> GetAll()
        {
            return _context.Users.Include(u => u.Roles).ToList();
        }

        public User GetById(string id)
        {
            return _context.Users.Include(u => u.Roles).FirstOrDefault(u => u.UserId == id);
        }

        public void Add(User user)
        {
            _context.Users.Add(user);
        }

        public void Update(User user)
        {
            _context.Users.Update(user);
        }

        public void Delete(string id)
        {
            var user = GetById(id);
            if (user != null)
            {
                _context.Users.Remove(user);
            }
        }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}
