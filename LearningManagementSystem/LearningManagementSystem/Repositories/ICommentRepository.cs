using LearningManagementSystem.Models;

namespace LearningManagementSystem.Repositories
{
    public interface ICommentRepository
    {
        IEnumerable<Comment> GetAll();
        Comment GetById(string commentId);
        IEnumerable<Comment> GetCommentsByCourse(string courseId);
        IEnumerable<Comment> GetCommentsByUser(string userId);
        IEnumerable<Comment> GetCommentsByLessonId(string lessonId); // Thêm phương thức mới
        void Add(Comment comment);
        void Update(Comment comment);
        void Delete(string commentId);
        void Save();
    }
}
