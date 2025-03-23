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

        public IEnumerable<Lesson> GetAll()
        {
            return _context.Lessons
                .Include(l => l.Course)
                .ToList();
        }

        public Lesson GetById(string lessonId)
        {
            return _context.Lessons
                .Include(l => l.Course)
                .FirstOrDefault(l => l.LessonId == lessonId);
        }

        public IEnumerable<Lesson> GetLessonsByCourse(string courseId)
        {
            return _context.Lessons
                .Include(l => l.Course)
                .Where(l => l.CourseId == courseId)
                .ToList();
        }

        public void Add(Lesson lesson)
        {
            _context.Lessons.Add(lesson);
        }

        public void Update(Lesson lesson)
        {
            _context.Lessons.Update(lesson);
        }

        public void Delete(string lessonId)
        {
            var lesson = _context.Lessons.FirstOrDefault(l => l.LessonId == lessonId);
            if (lesson != null)
            {
                _context.Lessons.Remove(lesson);
            }
        }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}