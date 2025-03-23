using LearningManagementSystem.Models;
using System.Collections.Generic;

namespace LearningManagementSystem.Repositories
{
    public interface ILessonRepository
    {
        IEnumerable<Lesson> GetAll();
        Lesson GetById(string lessonId);
        IEnumerable<Lesson> GetLessonsByCourse(string courseId);
        void Add(Lesson lesson);
        void Update(Lesson lesson);
        void Delete(string lessonId);
        void Save();
    }
}