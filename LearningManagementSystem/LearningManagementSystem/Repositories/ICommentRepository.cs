using LearningManagementSystem.Models;

namespace LearningManagementSystem.Repositories
{
    public interface ICommentRepository
    {
        Comment GetById(string commentId);
        IEnumerable<Comment> GetCommentsByLessonId(string lessonId);
        void Add(Comment comment);
        void Delete(Comment comment);
    }
}
