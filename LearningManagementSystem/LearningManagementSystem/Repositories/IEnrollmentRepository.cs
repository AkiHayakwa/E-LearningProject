using LearningManagementSystem.Models;

namespace LearningManagementSystem.Repositories
{
    public interface IEnrollmentRepository
    {
        IEnumerable<Enrollment> GetAll();
        Enrollment GetEnrollment(string userId, string courseId);
        IEnumerable<Enrollment> GetEnrollmentsByUser(string userId);
        void Add(Enrollment enrollment);
        void Delete(string enrollmentId);
        void Save();
    }
}
