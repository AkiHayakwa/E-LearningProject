using LearningManagementSystem.Data;
using LearningManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace LearningManagementSystem.Repositories
{
    public class EnrollmentRepository : IEnrollmentRepository
    {
        private readonly LMSContext _context;

        public EnrollmentRepository(LMSContext context)
        {
            _context = context;
        }

        public IQueryable<Enrollment> GetAll()
        {
            return _context.Enrollments
                .Include(e => e.User)
                .Include(e => e.Course)
                .AsQueryable();
        }

        public Enrollment GetById(string id)
        {
            return _context.Enrollments
                .Include(e => e.User)
                .Include(e => e.Course)
                .FirstOrDefault(e => e.EnrollmentId == id);
        }

        public Enrollment GetEnrollment(string userName, string courseId)
        {
            return _context.Enrollments
                .FirstOrDefault(e => e.UserName == userName && e.CourseId == courseId);
        }

        public IQueryable<Enrollment> GetEnrollmentsByUser(string userName)
        {
            return _context.Enrollments
                .Include(e => e.User)
                .Include(e => e.Course)
                .Where(e => e.UserName == userName)
                .AsQueryable();
        }

        public void Add(Enrollment enrollment)
        {
            _context.Enrollments.Add(enrollment);
        }

        public void Update(Enrollment enrollment)
        {
            _context.Enrollments.Update(enrollment);
        }

        public void Delete(string enrollmentId)
        {
            var enrollment = GetById(enrollmentId);
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