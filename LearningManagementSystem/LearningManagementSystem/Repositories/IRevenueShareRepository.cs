using LearningManagementSystem.Models;

namespace LearningManagementSystem.Repositories
{
    public interface IRevenueShareRepository
    {
        Task<IEnumerable<RevenueShare>> GetAllAsync();
        Task<RevenueShare?> GetByIdAsync(string id);
        Task<IEnumerable<RevenueShare>> GetByPaymentIdAsync(string paymentId);
        Task<IEnumerable<RevenueShare>> GetByUserNameAsync(string userName);
        Task<IEnumerable<RevenueShare>> GetByCourseIdAsync(string courseId);
        Task<RevenueShare> CreateAsync(RevenueShare revenueShare);
        Task<RevenueShare> UpdateAsync(RevenueShare revenueShare);
        Task DeleteAsync(string id);
        Task<bool> ExistsAsync(string id);
    }
}

