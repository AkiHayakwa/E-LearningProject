using LearningManagementSystem.Data;
using LearningManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LearningManagementSystem.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly LMSContext _context;

        public NotificationRepository(LMSContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Notification notification)
        {
            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();
        }

        public async Task AddRangeAsync(IEnumerable<Notification> notifications)
        {
            await _context.Notifications.AddRangeAsync(notifications);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Notification>> GetByUserNameAsync(string userName)
        {
            return await _context.Notifications
                .Where(n => n.UserName == userName)
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();
        }

        public async Task<List<Notification>> GetByUserNameAsync(string userName, int page, int pageSize)
        {
            return await _context.Notifications
                .Where(n => n.UserName == userName)
                .OrderByDescending(n => n.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(string userName)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserName == userName && !n.IsRead);
        }

        public async Task<List<Notification>> GetRecentNotificationsAsync(string userName, int count = 5)
        {
            return await _context.Notifications
                .Where(n => n.UserName == userName)
                .OrderByDescending(n => n.CreatedDate)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<Notification>> GetAllNotificationsAsync()
        {
            return await _context.Notifications
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();
        }

        public async Task<Notification> GetByIdAsync(string notificationId)
        {
            return await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == notificationId);
        }

        public async Task UpdateAsync(Notification notification)
        {
            _context.Notifications.Update(notification);
            await _context.SaveChangesAsync();
        }

        public async Task<List<string>> GetAllUserNamesAsync()
        {
            return await _context.Users
                .Select(u => u.UserName)
                .ToListAsync();
        }

        public async Task UpdateRangeAsync(IEnumerable<Notification> notifications)
        {
            _context.Notifications.UpdateRange(notifications);
            await _context.SaveChangesAsync();
        }

        public async Task MarkAllAsReadAsync(string userName)
        {
            var unreadNotifications = await _context.Notifications
                .Where(n => n.UserName == userName && !n.IsRead)
                .ToListAsync();
            
            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
            }
            
            if (unreadNotifications.Any())
            {
                await _context.SaveChangesAsync();
            }
        }
    }
}