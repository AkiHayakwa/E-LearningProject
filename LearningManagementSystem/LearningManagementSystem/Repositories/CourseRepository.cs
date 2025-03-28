using LearningManagementSystem.Data;
using LearningManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace LearningManagementSystem.Repositories
{
    public class CourseRepository : ICourseRepository
    {
        private readonly LMSContext _context;

        public CourseRepository(LMSContext context)
        {
            _context = context;
        }

        public IQueryable<Course> GetAll()
        {
            return _context.Courses
                .Include(c => c.Lessons); // Tải danh sách bài học
        }

        public Course GetById(string id)
        {
            return _context.Courses
                .Include(c => c.Lessons) // Tải danh sách bài học
                .FirstOrDefault(c => c.CourseId == id);
        }

        public void Add(Course course)
        {
            _context.Courses.Add(course);
        }

        public void Update(Course course)
        {
            _context.Courses.Update(course);
        }

        public void Delete(string id)
        {
            var course = _context.Courses.Find(id);
            if (course != null)
            {
                _context.Courses.Remove(course);
            }
        }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}