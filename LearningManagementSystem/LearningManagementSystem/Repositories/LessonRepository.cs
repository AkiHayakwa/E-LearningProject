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
                .Include(l => l.Progresses)
                .ToList();
        }

        public Lesson GetById(string lessonId)
        {
            return _context.Lessons
                .Include(l => l.Course)
                .Include(l => l.Progresses)
                .FirstOrDefault(l => l.LessonId == lessonId);
        }

        public IEnumerable<Lesson> GetLessonsByCourse(string courseId)
        {
            return _context.Lessons
                .Include(l => l.Course)
                .Include(l => l.Progresses)
                .Where(l => l.CourseId == courseId)
                .OrderBy(l => l.OrderNumber)
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
            var lesson = _context.Lessons
                .Include(l => l.Progresses)
                .FirstOrDefault(l => l.LessonId == lessonId);
            if (lesson != null)
            {
                if (lesson.Progresses != null && lesson.Progresses.Any())
                {
                    _context.Progresses.RemoveRange(lesson.Progresses);
                }
                _context.Lessons.Remove(lesson);
            }
        }

        public void Save()
        {
            _context.SaveChanges();
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}

