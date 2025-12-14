using LearningManagementSystem.Data;
using LearningManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace LearningManagementSystem.Repositories
{
    public class ForumRepository : IForumRepository
    {
        private readonly LMSContext _context;

        public ForumRepository(LMSContext context)
        {
            _context = context;
        }

        public IQueryable<Forum> GetAll()
        {
            return _context.Forums
                .Include(f => f.Course)
                .Include(f => f.Topics)
                .AsQueryable();
        }

        public Forum? GetById(string id)
        {
            return _context.Forums
                .Include(f => f.Course)
                .Include(f => f.Topics)
                .FirstOrDefault(f => f.ForumId == id);
        }

        public Forum? GetByCourseId(string courseId)
        {
            return _context.Forums
                .Include(f => f.Course)
                .Include(f => f.Topics)
                .FirstOrDefault(f => f.CourseId == courseId);
        }

        public void Add(Forum forum)
        {
            _context.Forums.Add(forum);
        }

        public void Update(Forum forum)
        {
            _context.Forums.Update(forum);
        }

        public void Delete(string id)
        {
            var forum = GetById(id);
            if (forum != null)
            {
                _context.Forums.Remove(forum);
            }
        }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}

