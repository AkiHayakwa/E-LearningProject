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

        public IEnumerable<Comment> GetAll()
        {
            return _context.Comments
                .Include(c => c.User)
                .Include(c => c.Lesson)
                .ThenInclude(l => l.Course)
                .ToList();
        }

        public Comment GetById(string commentId)
        {
            return _context.Comments
                .Include(c => c.User)
                .Include(c => c.Lesson)
                .ThenInclude(l => l.Course)
                .FirstOrDefault(c => c.CommentId == commentId);
        }

        public IEnumerable<Comment> GetCommentsByCourse(string courseId)
        {
            return _context.Comments
                .Include(c => c.User)
                .Include(c => c.Lesson)
                .ThenInclude(l => l.Course)
                .Where(c => c.Lesson.CourseId == courseId)
                .ToList();
        }

        public IEnumerable<Comment> GetCommentsByUser(string userId)
        {
            return _context.Comments
                .Include(c => c.User)
                .Include(c => c.Lesson)
                .ThenInclude(l => l.Course)
                .Where(c => c.UserId == userId)
                .ToList();
        }

        public IEnumerable<Comment> GetCommentsByLessonId(string lessonId)
        {
            return _context.Comments
                .Include(c => c.User)
                .Include(c => c.Lesson)
                .ThenInclude(l => l.Course)
                .Where(c => c.LessonId == lessonId)
                .ToList();
        }

        public void Add(Comment comment)
        {
            _context.Comments.Add(comment);
        }

        public void Update(Comment comment)
        {
            _context.Comments.Update(comment);
        }

        public void Delete(string commentId)
        {
            var comment = _context.Comments.FirstOrDefault(c => c.CommentId == commentId);
            if (comment != null)
            {
                _context.Comments.Remove(comment);
            }
        }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}
