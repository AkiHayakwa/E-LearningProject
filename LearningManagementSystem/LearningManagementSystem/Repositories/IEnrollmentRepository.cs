using LearningManagementSystem.Models;

namespace LearningManagementSystem.Repositories
{
    public interface IEnrollmentRepository
    {
        IEnumerable<Enrollment> GetEnrollmentsByUser(string userId); // Phương thức đã có trước đó
        IEnumerable<Enrollment> GetEnrollmentsByUserId(string userId); // Thêm phương thức mới
        bool IsEnrolled(string userId, string courseId);
        void Add(Enrollment enrollment);
    }
}
