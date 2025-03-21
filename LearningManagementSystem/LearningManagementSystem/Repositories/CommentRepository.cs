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

        // Triển khai phương thức lấy Comment theo ID
        public Comment GetById(string commentId)
        {
            return _context.Comments.FirstOrDefault(c => c.CommentId == commentId);
        }

        public IEnumerable<Comment> GetCommentsByLessonId(string lessonId)
        {
            return _context.Comments.Include(c => c.User).Where(c => c.LessonId == lessonId).ToList();
        }

        public void Add(Comment comment)
        {
            _context.Comments.Add(comment);
        }

        public void Delete(Comment comment)
        {
            _context.Comments.Remove(comment);
        }
    }
}
