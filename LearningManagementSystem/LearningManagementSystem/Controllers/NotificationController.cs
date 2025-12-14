using LearningManagementSystem.Models;
using LearningManagementSystem.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace LearningManagementSystem.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IUserRepository _userRepository;

        public NotificationController(INotificationRepository notificationRepository, IUserRepository userRepository)
        {
            _notificationRepository = notificationRepository;
            _userRepository = userRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int page = 1)
        {
            var userName = User.Identity.Name;
            if (string.IsNullOrEmpty(userName))
            {
                return Unauthorized("Bạn cần đăng nhập để xem thông báo.");
            }

            int pageSize = 10; // Giảm từ 20 xuống 10 để hiển thị gọn hơn
            page = page < 1 ? 1 : page;

            // Tối ưu: chỉ query một lần để lấy count
            var allNotifications = await _notificationRepository.GetByUserNameAsync(userName);
            var totalCount = allNotifications.Count;
            var totalPages = totalCount > 0 ? (int)Math.Ceiling((double)totalCount / pageSize) : 1;

            // Đảm bảo page không vượt quá totalPages
            if (page > totalPages && totalPages > 0)
            {
                page = totalPages;
            }

            var notifications = await _notificationRepository.GetByUserNameAsync(userName, page, pageSize);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            ViewBag.UnreadCount = await _notificationRepository.GetUnreadCountAsync(userName);

            return View(notifications);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "Thông báo không hợp lệ.";
                return RedirectToAction("Index");
            }

            var notification = await _notificationRepository.GetByIdAsync(id);
            if (notification == null)
            {
                TempData["Error"] = "Không tìm thấy thông báo.";
                return RedirectToAction("Index");
            }

            var userName = User.Identity.Name;
            if (notification.UserName != userName)
            {
                TempData["Error"] = "Bạn không có quyền chỉnh sửa thông báo này.";
                return RedirectToAction("Index");
            }

            notification.IsRead = true;
            await _notificationRepository.UpdateAsync(notification);
            TempData["Success"] = "Đã đánh dấu thông báo là đã đọc.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userName = User.Identity.Name;
            if (string.IsNullOrEmpty(userName))
                return Unauthorized();

            await _notificationRepository.MarkAllAsReadAsync(userName);
            TempData["Success"] = "Đã đánh dấu tất cả thông báo là đã đọc.";
            return RedirectToAction("Index");
        }

        // API endpoint để lấy thông báo cho dropdown (AJAX)
        [HttpGet]
        public async Task<IActionResult> GetNotifications(int count = 5)
        {
            var userName = User.Identity.Name;
            if (string.IsNullOrEmpty(userName))
            {
                return Json(new { unreadCount = 0, notifications = new List<object>() });
            }

            var unreadCount = await _notificationRepository.GetUnreadCountAsync(userName);
            var notifications = await _notificationRepository.GetRecentNotificationsAsync(userName, count);

            var result = notifications.Select(n => new
            {
                id = n.NotificationId,
                title = n.Title,
                content = n.Content,
                createdDate = n.CreatedDate.ToString("dd/MM/yyyy HH:mm"),
                isRead = n.IsRead
            }).ToList();

            return Json(new { unreadCount, notifications = result });
        }

        #region Quản lý Thông báo
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SendNotification(int page = 1)
        {
            // Lấy danh sách user không phải Admin
            var users = _userRepository.GetAll().Where(u => u.Role != null && u.Role.RoleName != "Admin").ToList();
            ViewBag.UserNames = users.Select(u => u.UserName).ToList();

            // Lấy thông báo cho user hiện tại (dành cho layout hoặc header)
            if (User.Identity.IsAuthenticated)
            {
                var userName = User.Identity.Name;
                ViewBag.UnreadCount = await _notificationRepository.GetUnreadCountAsync(userName);
                ViewBag.Notifications = await _notificationRepository.GetRecentNotificationsAsync(userName, 3);
            }
            else
            {
                ViewBag.UnreadCount = 0;
                ViewBag.Notifications = new List<Notification>();
            }

            // Lấy tất cả thông báo đã gửi để hiển thị trong bảng (phân trang)
            var allNotifications = await _notificationRepository.GetAllNotificationsAsync();
            int pageSize = 10;
            int totalNotifications = allNotifications.Count;
            int totalPages = (int)Math.Ceiling((double)totalNotifications / pageSize);
            var pagedNotifications = allNotifications.OrderByDescending(n => n.CreatedDate)
                                                     .Skip((page - 1) * pageSize)
                                                     .Take(pageSize)
                                                     .ToList();
            ViewBag.AllNotifications = pagedNotifications;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View("~/Views/Admin/Notification/SendNotification.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SendNotification(string title, string content, string userName, bool sendToAll)
        {
            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(content))
            {
                TempData["Error"] = "Tiêu đề và nội dung không được để trống.";
                return RedirectToAction("SendNotification");
            }

            try
            {
                if (sendToAll)
                {
                    // Lấy tất cả user không phải Admin
                    var users = _userRepository.GetAll().Where(u => u.Role != null && u.Role.RoleName != "Admin").ToList();
                    var notifications = users.Select(user => new Notification
                    {
                        NotificationId = Guid.NewGuid().ToString(),
                        UserName = user.UserName,
                        Title = title,
                        Content = content,
                        CreatedDate = DateTime.UtcNow,
                        IsRead = false
                    }).ToList();

                    await _notificationRepository.AddRangeAsync(notifications);
                    TempData["Success"] = "Thông báo đã được gửi đến tất cả người dùng (trừ Admin)!";
                }
                else
                {
                    if (string.IsNullOrEmpty(userName))
                    {
                        TempData["Error"] = "Vui lòng chọn người dùng để gửi thông báo.";
                        return RedirectToAction("SendNotification");
                    }
                    // Kiểm tra user có phải Admin không
                    var user = _userRepository.GetByUserName(userName);
                    if (user != null && user.Role != null && user.Role.RoleName == "Admin")
                    {
                        TempData["Error"] = "Không thể gửi thông báo cho tài khoản Admin.";
                        return RedirectToAction("SendNotification");
                    }

                    var notification = new Notification
                    {
                        NotificationId = Guid.NewGuid().ToString(),
                        UserName = userName,
                        Title = title,
                        Content = content,
                        CreatedDate = DateTime.UtcNow,
                        IsRead = false
                    };

                    await _notificationRepository.AddAsync(notification);
                    TempData["Success"] = $"Thông báo đã được gửi đến {userName}!";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi khi gửi thông báo: " + ex.Message;
            }

            return RedirectToAction("SendNotification");
        }
        #endregion
    }
}