using LearningManagementSystem.Data;
using LearningManagementSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;

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

        public IQueryable<User> GetAll()
        {
            return _context.Users
                .Include(u => u.Role) // Sửa từ Roles thành Role
                .Include(u => u.Comments)
                .Include(u => u.Enrollments)
                .Include(u => u.Progresses)
                .AsQueryable();
        }

        public User GetByUserName(string userName)
        {
            return _context.Users
                .Include(u => u.Role)
                .FirstOrDefault(u => u.UserName == userName);
        }

        public void Add(User user)
        {
            _context.Users.Add(user);
        }

        public void Update(User user)
        {
            _context.Users.Update(user);
        }

        public void Delete(string userName)
        {
            var user = GetByUserName(userName);
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