using LearningManagementSystem.Data;
using LearningManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace LearningManagementSystem.Repositories
{
    public class LessonRepository : ILessonRepository
    {
        private readonly LMSContext _context;

        public LessonRepository(LMSContext context)
        {
            _context = context;
        }

        public IEnumerable<Lesson> GetLessonsByCourseId(string courseId)
        {
            var query = _context.Lessons
                                .Include(l => l.Course)
                                .AsQueryable();

            if (!string.IsNullOrEmpty(courseId))
            {
                query = query.Where(l => l.CourseId == courseId);
            }

            return query.ToList();
        }

        public Lesson GetById(string lessonId) // Đổi từ GetLessonById thành GetById
        {
            return _context.Lessons
                           .Include(l => l.Course)
                           .FirstOrDefault(l => l.LessonId == lessonId);
        }

        public void Add(Lesson lesson)
        {
            _context.Lessons.Add(lesson);
            _context.SaveChanges();
        }

        public void Update(Lesson lesson)
        {
            _context.Lessons.Update(lesson);
            _context.SaveChanges();
        }

        public void Delete(Lesson lesson)
        {
            _context.Lessons.Remove(lesson);
            _context.SaveChanges();
        }
    }
}