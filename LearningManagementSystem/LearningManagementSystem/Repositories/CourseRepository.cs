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

        // Constructor nhận DbContext qua DI
        public CourseRepository(LMSContext context)
        {
            _context = context;
        }

        public IQueryable<Course> GetAll()
        {
            return _context.Courses.AsQueryable();
        }

        public Course GetById(string courseId)
        {
            return _context.Courses
                .FirstOrDefault(c => c.CourseId == courseId);
        }

        public void Add(Course course)
        {
            _context.Courses.Add(course);
        }

        public void Update(Course course)
        {
            _context.Courses.Update(course);
        }

        public void Delete(string courseId)
        {
            var course = _context.Courses.FirstOrDefault(c => c.CourseId == courseId);
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