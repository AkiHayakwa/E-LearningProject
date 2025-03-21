using LearningManagementSystem.Models;

namespace LearningManagementSystem.Repositories
{
    public interface IEnrollmentRepository
    {
        Enrollment GetById(string enrollmentId);
        IEnumerable<Enrollment> GetEnrollmentsByUserId(string userId);
        void Add(Enrollment enrollment);
        void Delete(Enrollment enrollment);
    }
}
