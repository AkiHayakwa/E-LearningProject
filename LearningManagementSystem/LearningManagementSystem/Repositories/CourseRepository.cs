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

        public IEnumerable<Course> GetAll()
        {
            return _context.Courses
                .Include(c => c.Instructor)
                .ToList();
        }

        public Course GetById(string courseId)
        {
            return _context.Courses
                .Include(c => c.Instructor)
                .FirstOrDefault(c => c.CourseId == courseId);
        }

        public IEnumerable<Course> GetCoursesByInstructor(string instructorId)
        {
            return _context.Courses
                .Include(c => c.Instructor)
                .Where(c => c.InstructorId == instructorId)
                .ToList();
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
