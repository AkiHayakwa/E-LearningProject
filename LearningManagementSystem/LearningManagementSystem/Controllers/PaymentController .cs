using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LearningManagementSystem.Data;
using LearningManagementSystem.Models;
using LearningManagementSystem.Models.ViewModels;
using LearningManagementSystem.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearningManagementSystem.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly LMSContext _context;
        private readonly IRevenueShareRepository _revenueShareRepository;
        private readonly IUserRepository _userRepository;

        public PaymentController(LMSContext context, IRevenueShareRepository revenueShareRepository, IUserRepository userRepository)
        {
            _context = context;
            _revenueShareRepository = revenueShareRepository;
            _userRepository = userRepository;
        }

        // GET: Payment/History
        public async Task<IActionResult> History()
        {
            var username = User.Identity.Name;

            var payments = await _context.Payments
                .Include(p => p.Course)
                .Where(p => p.UserName == username)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            return View(payments);
        }

        // GET: Payment/PaymentDetail/5
        public async Task<IActionResult> PaymentDetail(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var username = User.Identity.Name;

            var payment = await _context.Payments
                .Include(p => p.Course)
                .FirstOrDefaultAsync(p => p.PaymentId == id && p.UserName == username);

            if (payment == null)
            {
                return NotFound();
            }

            // Nếu là giao dịch mua nhiều khóa học, lấy chi tiết từ OrderDetails
            var orderDetails = await _context.OrderDetails
                .Include(od => od.Course)
                .Where(od => od.PaymentId == id)
                .ToListAsync();

            var viewModel = new PaymentDetailViewModel
            {
                Payment = payment,
                OrderDetails = orderDetails
            };

            return View(viewModel);
        }

        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> ManagePayment(int page = 1)
        {
            var username = User.Identity.Name;
            var isAdmin = User.IsInRole("Admin");

            var query = _context.Payments
                .Include(p => p.User)
                .Include(p => p.OrderDetails)
                    .ThenInclude(od => od.Course)
                        .ThenInclude(c => c.CourseInstructors)
                .AsQueryable();

            // Nếu là instructor, chỉ lấy các thanh toán liên quan đến khóa học của họ
            if (!isAdmin)
            {
                query = query.Where(p => p.OrderDetails.Any(od => 
                    od.Course.CourseInstructors.Any(ci => ci.UserName == username)));
            }

            int pageSize = 10;
            int totalPayments = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalPayments / pageSize);
            var payments = await query
                .OrderByDescending(p => p.PaymentDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(payments);
        }

       [HttpPost]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> UpdatePaymentStatus(string paymentId, string newStatus)
        {
            var username = User.Identity.Name;
            var isAdmin = User.IsInRole("Admin");

            var payment = await _context.Payments
                .Include(p => p.OrderDetails)
                    .ThenInclude(od => od.Course)
                        .ThenInclude(c => c.CourseInstructors)
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.PaymentId == paymentId);

            if (payment == null)
            {
                return NotFound();
            }

            // Kiểm tra quyền cập nhật
            if (!isAdmin)
            {
                var hasPermission = payment.OrderDetails.Any(od => 
                    od.Course.CourseInstructors.Any(ci => ci.UserName == username));
                
                if (!hasPermission)
                {
                    return Forbid();
                }
            }

            payment.PaymentStatus = newStatus;
            await _context.SaveChangesAsync();

            // Nếu trạng thái thanh toán thành công, tạo enrollment cho user và chia doanh thu
            if (newStatus == "Completed")
            {
                foreach (var orderDetail in payment.OrderDetails)
                {
                    // Kiểm tra xem user đã đăng ký khóa học này chưa
                    var existingEnrollment = await _context.Enrollments
                        .FirstOrDefaultAsync(e => e.UserName == payment.UserName && e.CourseId == orderDetail.CourseId);

                    if (existingEnrollment == null)
                    {
                        // Tạo enrollment mới
                        var enrollment = new Enrollment
                        {
                            EnrollmentId = Guid.NewGuid().ToString(),
                            UserName = payment.UserName,
                            CourseId = orderDetail.CourseId,
                            EnrollmentDate = DateTime.Now
                        };

                        _context.Enrollments.Add(enrollment);

                        // Tạo thông báo cho user
                        var notification = new Notification
                        {
                            NotificationId = Guid.NewGuid().ToString(),
                            UserName = payment.UserName,
                            Title = "Đăng ký khóa học thành công",
                            Content = $"Bạn đã được đăng ký thành công vào khóa học: {orderDetail.Course.CourseName}",
                            CreatedDate = DateTime.Now,
                            IsRead = false
                        };

                        _context.Notifications.Add(notification);
                    }

                    // Chia doanh thu cho giảng viên và admin
                    await ProcessRevenueShare(payment.PaymentId, orderDetail.CourseId, orderDetail.Price);
                }
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "Cập nhật trạng thái thanh toán thành công!";
            return RedirectToAction("ManagePayment");
        }

        private async Task ProcessRevenueShare(string paymentId, string courseId, decimal coursePrice)
        {
            // Lấy danh sách giảng viên của khóa học
            var instructors = await _context.CourseInstructors
                .Where(ci => ci.CourseId == courseId)
                .ToListAsync();

            if (instructors.Any())
            {
                // Tính toán số tiền chia cho từng giảng viên (80% tổng chia đều)
                decimal instructorShare = coursePrice * 0.8m;
                decimal instructorAmount = instructorShare / instructors.Count;

                // Tạo revenue share cho từng giảng viên
                foreach (var instructor in instructors)
                {
                    var instructorRevenueShare = new RevenueShare
                    {
                        RevenueShareId = Guid.NewGuid().ToString(),
                        PaymentId = paymentId,
                        UserName = instructor.UserName,
                        CourseId = courseId,
                        Amount = instructorAmount,
                        Percentage = 80m / instructors.Count,
                        ShareType = "Instructor",
                        CreatedDate = DateTime.Now
                    };

                    await _revenueShareRepository.CreateAsync(instructorRevenueShare);

                    // Cộng tiền vào ví giảng viên
                    var instructorUser = _userRepository.GetByUserName(instructor.UserName);
                    if (instructorUser != null)
                    {
                        instructorUser.WalletBalance += instructorAmount;
                        _userRepository.Update(instructorUser);
                    }
                }
            }

            // Tạo revenue share cho admin (20%)
            decimal adminAmount = coursePrice * 0.2m;
            var adminRevenueShare = new RevenueShare
            {
                RevenueShareId = Guid.NewGuid().ToString(),
                PaymentId = paymentId,
                UserName = "admin", // Tài khoản admin mặc định
                CourseId = courseId,
                Amount = adminAmount,
                Percentage = 20m,
                ShareType = "Admin",
                CreatedDate = DateTime.Now
            };

            await _revenueShareRepository.CreateAsync(adminRevenueShare);

            // Cộng tiền vào ví admin
            var adminUser = _userRepository.GetByUserName("admin");
            if (adminUser != null)
            {
                adminUser.WalletBalance += adminAmount;
                _userRepository.Update(adminUser);
            }
        }
    }
}