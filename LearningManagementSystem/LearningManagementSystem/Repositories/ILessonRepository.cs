using LearningManagementSystem.Models;

namespace LearningManagementSystem.Repositories
{
    public interface ILessonRepository
    {
        Lesson GetById(string lessonId);
        IEnumerable<Lesson> GetLessonsByCourseId(string courseId);
        void Add(Lesson lesson);
        void Update(Lesson lesson);
        void Delete(Lesson lesson);
    }
}
