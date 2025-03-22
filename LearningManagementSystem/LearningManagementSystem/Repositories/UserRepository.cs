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

        public User GetUserWithRole(string userId)
        {
            return _context.Users
                           .Include(u => u.Roles)
                           .FirstOrDefault(u => u.UserId == userId);
        }

        public IEnumerable<User> GetAll()
        {
            return _context.Users
                           .Include(u => u.Roles)
                           .ToList();
        }

        public User GetById(string id)
        {
            return _context.Users
                           .Include(u => u.Roles)
                           .FirstOrDefault(u => u.UserId == id);
        }

        public void Add(User user, string plainPassword)
        {
            // Băm mật khẩu trước khi lưu
            user.HashPassword(_passwordHasher, plainPassword);
            _context.Users.Add(user);
            _context.SaveChanges();
        }

        public void Delete(User user)
        {
            _context.Users.Remove(user);
            _context.SaveChanges();
        }

        public User GetByUserName(string userName)
        {
            return _context.Users
                           .Include(u => u.Roles)
                           .FirstOrDefault(u => u.UserName == userName);
        }
    }
}
