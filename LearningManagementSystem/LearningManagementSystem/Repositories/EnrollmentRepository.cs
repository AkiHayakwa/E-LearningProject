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

        public IEnumerable<Enrollment> GetAll()
        {
            return _context.Enrollments
                .Include(e => e.User)
                .Include(e => e.Course)
                .ToList();
        }

        public Enrollment GetEnrollment(string userId, string courseId)
        {
            return _context.Enrollments
                .FirstOrDefault(e => e.UserId == userId && e.CourseId == courseId);
        }

        public IEnumerable<Enrollment> GetEnrollmentsByUser(string userId)
        {
            return _context.Enrollments
                .Include(e => e.User)
                .Include(e => e.Course)
                .Where(e => e.UserId == userId)
                .ToList();
        }

        public void Add(Enrollment enrollment)
        {
            _context.Enrollments.Add(enrollment);
        }

        public void Delete(string enrollmentId)
        {
            var enrollment = _context.Enrollments.FirstOrDefault(e => e.EnrollmentId == enrollmentId);
            if (enrollment != null)
            {
                _context.Enrollments.Remove(enrollment);
            }
        }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}
