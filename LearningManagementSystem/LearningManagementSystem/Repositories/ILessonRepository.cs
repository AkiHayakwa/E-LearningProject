using LearningManagementSystem.Models;
using System.Collections.Generic;

namespace LearningManagementSystem.Repositories
{
    public interface ILessonRepository
    {
        IEnumerable<Lesson> GetLessonsByCourseId(string courseId);
        Lesson GetById(string lessonId); // Đổi từ GetLessonById thành GetById
        void Add(Lesson lesson);
        void Update(Lesson lesson);
        void Delete(Lesson lesson);
    }
}