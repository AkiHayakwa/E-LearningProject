using LearningManagementSystem.Data;
using LearningManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

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

        public Course GetCourseWithInstructor(string courseId)
        {
            return _context.Courses
                           .Include(c => c.Instructor)
                           .FirstOrDefault(c => c.CourseId == courseId);
        }

        public IEnumerable<Course> GetAllWithInstructor()
        {
            return _context.Courses
                           .Include(c => c.Instructor)
                           .ToList();
        }

        public IEnumerable<Course> GetAll()
        {
            return _context.Courses.ToList();
        }

        public Course GetById(string id)
        {
            return _context.Courses.FirstOrDefault(c => c.CourseId == id);
        }

        public void Add(Course course)
        {
            _context.Courses.Add(course);
            _context.SaveChanges();
        }

        public void Delete(Course course)
        {
            _context.Courses.Remove(course);
            _context.SaveChanges();
        }
    }
}
