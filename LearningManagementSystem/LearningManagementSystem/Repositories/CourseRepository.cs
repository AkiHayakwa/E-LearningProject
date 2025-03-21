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

        public Course GetById(string courseId)
        {
            return _context.Courses.Include(c => c.Lessons).FirstOrDefault(c => c.CourseId == courseId);
        }

        public IEnumerable<Course> GetAll()
        {
            return _context.Courses.Include(c => c.Instructor).ToList();
        }

        public void Add(Course course)
        {
            _context.Courses.Add(course);
        }

        public void Update(Course course)
        {
            _context.Courses.Update(course);
        }

        public void Delete(Course course)
        {
            _context.Courses.Remove(course);
        }
    }
}
