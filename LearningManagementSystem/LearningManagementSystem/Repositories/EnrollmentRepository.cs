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

        public Enrollment GetById(string enrollmentId)
        {
            return _context.Enrollments.FirstOrDefault(e => e.EnrollmentId == enrollmentId);
        }

        public IEnumerable<Enrollment> GetEnrollmentsByUserId(string userId)
        {
            return _context.Enrollments.Include(e => e.Course).Where(e => e.UserId == userId).ToList();
        }

        public void Add(Enrollment enrollment)
        {
            _context.Enrollments.Add(enrollment);
        }

        public void Delete(Enrollment enrollment)
        {
            _context.Enrollments.Remove(enrollment);
        }
    }
}
