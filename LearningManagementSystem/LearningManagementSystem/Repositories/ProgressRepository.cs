using LearningManagementSystem.Data;
using LearningManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace LearningManagementSystem.Repositories
{
    public class ProgressRepository : IProgressRepository
    {
        private readonly LMSContext _context;

        public ProgressRepository(LMSContext context)
        {
            _context = context;
        }

        public IEnumerable<Progress> GetProgressByUser(string userId)
        {
            var query = _context.Progresses
                                .Include(p => p.User)
                                .Include(p => p.Lesson)
                                .AsQueryable();

            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(p => p.UserId == userId);
            }

            return query.ToList();
        }

        public IEnumerable<Progress> GetProgressByUserAndCourse(string userId, string courseId)
        {
            var query = _context.Progresses
                                .Include(p => p.User)
                                .Include(p => p.Lesson)
                                    .ThenInclude(l => l.Course)
                                .AsQueryable();

            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(p => p.UserId == userId);
            }

            if (!string.IsNullOrEmpty(courseId))
            {
                query = query.Where(p => p.Lesson.CourseId == courseId);
            }

            return query.ToList();
        }

        public Progress GetById(string progressId)
        {
            return _context.Progresses
                           .Include(p => p.User)
                           .Include(p => p.Lesson)
                           .FirstOrDefault(p => p.ProgressId == progressId);
        }

        public void Add(Progress progress)
        {
            _context.Progresses.Add(progress);
            _context.SaveChanges();
        }

        public void Update(Progress progress)
        {
            _context.Progresses.Update(progress);
            _context.SaveChanges();
        }

        public void Delete(Progress progress)
        {
            _context.Progresses.Remove(progress);
            _context.SaveChanges();
        }
    }
}