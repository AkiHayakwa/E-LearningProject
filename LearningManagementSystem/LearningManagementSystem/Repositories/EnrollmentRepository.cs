using LearningManagementSystem.Data;
using LearningManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace LearningManagementSystem.Repositories
{
    public class EnrollmentRepository : IEnrollmentRepository
    {
        private readonly LMSContext _context;

        public EnrollmentRepository(LMSContext context)
        {
            _context = context;
        }

        public IEnumerable<Enrollment> GetEnrollmentsByUser(string userId)
        {
            return _context.Enrollments
                           .Include(e => e.Course)
                           .Where(e => e.UserId == userId)
                           .ToList();
        }

        public IEnumerable<Enrollment> GetEnrollmentsByUserId(string userId)
        {
            return _context.Enrollments
                           .Where(e => e.UserId == userId)
                           .ToList();
        }

        public bool IsEnrolled(string userId, string courseId)
        {
            return _context.Enrollments
                           .Any(e => e.UserId == userId && e.CourseId == courseId);
        }

        public void Add(Enrollment enrollment)
        {
            _context.Enrollments.Add(enrollment);
            _context.SaveChanges();
        }
    }
}
