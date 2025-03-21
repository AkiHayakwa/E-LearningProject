using LearningManagementSystem.Data;
using LearningManagementSystem.Models;

namespace LearningManagementSystem.Repositories
{
    public class LessonRepository : ILessonRepository
    {
        private readonly LMSContext _context;

        // Constructor nhận DbContext qua DI
        public LessonRepository(LMSContext context)
        {
            _context = context;
        }

        public Lesson GetById(string lessonId)
        {
            return _context.Lessons.FirstOrDefault(l => l.LessonId == lessonId);
        }

        public IEnumerable<Lesson> GetLessonsByCourseId(string courseId)
        {
            return _context.Lessons.Where(l => l.CourseId == courseId).OrderBy(l => l.OrderNumber).ToList();
        }

        public void Add(Lesson lesson)
        {
            _context.Lessons.Add(lesson);
        }

        public void Update(Lesson lesson)
        {
            _context.Lessons.Update(lesson);
        }

        public void Delete(Lesson lesson)
        {
            _context.Lessons.Remove(lesson);
        }
    }
}
