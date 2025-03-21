using LearningManagementSystem.Data;
using LearningManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace LearningManagementSystem.Repositories
{
    public class ProgressRepository : IProgressRepository
    {
        private readonly LMSContext _context;

        // Constructor nhận DbContext qua DI
        public ProgressRepository(LMSContext context)
        {
            _context = context;
        }

        public IEnumerable<Progress> GetProgressByUserAndCourse(string userId, string courseId)
        {
            return _context.Progresses.Include(p => p.Lesson)
                                      .Where(p => p.UserId == userId && p.Lesson.CourseId == courseId)
                                      .ToList();
        }

        public void Add(Progress progress)
        {
            _context.Progresses.Add(progress);
        }

        public void Update(Progress progress)
        {
            _context.Progresses.Update(progress);
        }
    }
}
