using LearningManagementSystem.Models;

namespace LearningManagementSystem.Repositories
{
    public interface ICommentRepository
    {
        IEnumerable<Comment> GetCommentsByCourse(string courseId);
        IEnumerable<Comment> GetRecentComments(string userId, bool isNotification);
        IEnumerable<Comment> GetCommentsByLessonId(string lessonId);
        Comment GetById(string id);
        void Add(Comment comment);
        void Delete(Comment comment);
    }
}
