using LearningManagementSystem.Data;
using LearningManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace LearningManagementSystem.Repositories
{
    public class RevenueShareRepository : IRevenueShareRepository
    {
        private readonly LMSContext _context;

        public RevenueShareRepository(LMSContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<RevenueShare>> GetAllAsync()
        {
            return await _context.RevenueShares
                .Include(rs => rs.Payment)
                .Include(rs => rs.User)
                .Include(rs => rs.Course)
                .ToListAsync();
        }

        public async Task<RevenueShare?> GetByIdAsync(string id)
        {
            return await _context.RevenueShares
                .Include(rs => rs.Payment)
                .Include(rs => rs.User)
                .Include(rs => rs.Course)
                .FirstOrDefaultAsync(rs => rs.RevenueShareId == id);
        }

        public async Task<IEnumerable<RevenueShare>> GetByPaymentIdAsync(string paymentId)
        {
            return await _context.RevenueShares
                .Include(rs => rs.Payment)
                .Include(rs => rs.User)
                .Include(rs => rs.Course)
                .Where(rs => rs.PaymentId == paymentId)
                .ToListAsync();
        }

        public async Task<IEnumerable<RevenueShare>> GetByUserNameAsync(string userName)
        {
            return await _context.RevenueShares
                .Include(rs => rs.Payment)
                .Include(rs => rs.User)
                .Include(rs => rs.Course)
                .Where(rs => rs.UserName == userName)
                .ToListAsync();
        }

        public async Task<IEnumerable<RevenueShare>> GetByCourseIdAsync(string courseId)
        {
            return await _context.RevenueShares
                .Include(rs => rs.Payment)
                .Include(rs => rs.User)
                .Include(rs => rs.Course)
                .Where(rs => rs.CourseId == courseId)
                .ToListAsync();
        }

        public async Task<RevenueShare> CreateAsync(RevenueShare revenueShare)
        {
            _context.RevenueShares.Add(revenueShare);
            await _context.SaveChangesAsync();
            return revenueShare;
        }

        public async Task<RevenueShare> UpdateAsync(RevenueShare revenueShare)
        {
            _context.RevenueShares.Update(revenueShare);
            await _context.SaveChangesAsync();
            return revenueShare;
        }

        public async Task DeleteAsync(string id)
        {
            var revenueShare = await _context.RevenueShares.FindAsync(id);
            if (revenueShare != null)
            {
                _context.RevenueShares.Remove(revenueShare);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(string id)
        {
            return await _context.RevenueShares.AnyAsync(rs => rs.RevenueShareId == id);
        }
    }
}

