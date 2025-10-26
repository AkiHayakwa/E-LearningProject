using LearningManagementSystem.Data;
using LearningManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace LearningManagementSystem.Repositories
{
    public class WithdrawalRequestRepository : IWithdrawalRequestRepository
    {
        private readonly LMSContext _context;

        public WithdrawalRequestRepository(LMSContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<WithdrawalRequest>> GetAllAsync()
        {
            return await _context.WithdrawalRequests
                .Include(wr => wr.User)
                .Include(wr => wr.ProcessedByUser)
                .OrderByDescending(wr => wr.RequestDate)
                .ToListAsync();
        }

        public async Task<WithdrawalRequest?> GetByIdAsync(string id)
        {
            return await _context.WithdrawalRequests
                .Include(wr => wr.User)
                .Include(wr => wr.ProcessedByUser)
                .FirstOrDefaultAsync(wr => wr.WithdrawalRequestId == id);
        }

        public async Task<IEnumerable<WithdrawalRequest>> GetByUserNameAsync(string userName)
        {
            return await _context.WithdrawalRequests
                .Include(wr => wr.User)
                .Include(wr => wr.ProcessedByUser)
                .Where(wr => wr.UserName == userName)
                .OrderByDescending(wr => wr.RequestDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<WithdrawalRequest>> GetByStatusAsync(string status)
        {
            return await _context.WithdrawalRequests
                .Include(wr => wr.User)
                .Include(wr => wr.ProcessedByUser)
                .Where(wr => wr.Status == status)
                .OrderByDescending(wr => wr.RequestDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<WithdrawalRequest>> GetPendingRequestsAsync()
        {
            return await _context.WithdrawalRequests
                .Include(wr => wr.User)
                .Include(wr => wr.ProcessedByUser)
                .Where(wr => wr.Status == "Pending")
                .OrderByDescending(wr => wr.RequestDate)
                .ToListAsync();
        }

        public async Task<WithdrawalRequest> CreateAsync(WithdrawalRequest withdrawalRequest)
        {
            _context.WithdrawalRequests.Add(withdrawalRequest);
            await _context.SaveChangesAsync();
            return withdrawalRequest;
        }

        public async Task<WithdrawalRequest> UpdateAsync(WithdrawalRequest withdrawalRequest)
        {
            _context.WithdrawalRequests.Update(withdrawalRequest);
            await _context.SaveChangesAsync();
            return withdrawalRequest;
        }

        public async Task DeleteAsync(string id)
        {
            var withdrawalRequest = await _context.WithdrawalRequests.FindAsync(id);
            if (withdrawalRequest != null)
            {
                _context.WithdrawalRequests.Remove(withdrawalRequest);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(string id)
        {
            return await _context.WithdrawalRequests.AnyAsync(wr => wr.WithdrawalRequestId == id);
        }
    }
}

