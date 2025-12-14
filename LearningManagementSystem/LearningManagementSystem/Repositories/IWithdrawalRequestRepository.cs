using LearningManagementSystem.Models;

namespace LearningManagementSystem.Repositories
{
    public interface IWithdrawalRequestRepository
    {
        Task<IEnumerable<WithdrawalRequest>> GetAllAsync();
        Task<WithdrawalRequest?> GetByIdAsync(string id);
        Task<IEnumerable<WithdrawalRequest>> GetByUserNameAsync(string userName);
        Task<IEnumerable<WithdrawalRequest>> GetByStatusAsync(string status);
        Task<IEnumerable<WithdrawalRequest>> GetPendingRequestsAsync();
        Task<WithdrawalRequest> CreateAsync(WithdrawalRequest withdrawalRequest);
        Task<WithdrawalRequest> UpdateAsync(WithdrawalRequest withdrawalRequest);
        Task DeleteAsync(string id);
        Task<bool> ExistsAsync(string id);
        Task<IEnumerable<WithdrawalRequest>> GetApprovedByUserNameAsync(string userName);
        Task<IEnumerable<WithdrawalRequest>> GetAllPendingAsync();

    }
}

