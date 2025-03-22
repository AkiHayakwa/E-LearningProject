using LearningManagementSystem.Data;
using LearningManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace LearningManagementSystem.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        private readonly LMSContext _context;

        // Constructor nhận DbContext qua DI
        public CommentRepository(LMSContext context)
        {
            _context = context;
        }

        public IEnumerable<Comment> GetCommentsByCourse(string courseId)
        {
            return _context.Comments
                           .Include(c => c.User)
                           .Where(c => c.CourseId == courseId && c.LessonId == null)
                           .OrderByDescending(c => c.CreatedDate)
                           .ToList();
        }

        public IEnumerable<Comment> GetRecentComments(string userId, bool isNotification)
        {
            if (isNotification)
            {
                return _context.Comments
                               .Where(c => c.LessonId == "notification" && c.UserId == userId)
                               .OrderByDescending(c => c.CreatedDate)
                               .Take(5)
                               .ToList();
            }
            else
            {
                return _context.Comments
                               .Include(c => c.User)
                               .Include(c => c.Lesson)
                               .ThenInclude(l => l.Course)
                               .Where(c => c.LessonId != "notification")
                               .OrderByDescending(c => c.CreatedDate)
                               .Take(5)
                               .ToList();
            }
        }

        public IEnumerable<Comment> GetCommentsByLessonId(string lessonId)
        {
            return _context.Comments
                           .Include(c => c.User)
                           .Where(c => c.LessonId == lessonId)
                           .OrderByDescending(c => c.CreatedDate)
                           .ToList();
        }

        public Comment GetById(string id)
        {
            return _context.Comments.FirstOrDefault(c => c.CommentId == id);
        }

        public void Add(Comment comment)
        {
            _context.Comments.Add(comment);
            _context.SaveChanges();
        }

        public void Delete(Comment comment)
        {
            _context.Comments.Remove(comment);
            _context.SaveChanges();
        }
    }
}
