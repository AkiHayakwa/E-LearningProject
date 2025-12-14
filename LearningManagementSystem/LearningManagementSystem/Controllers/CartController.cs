using Microsoft.AspNetCore.Mvc;
using LearningManagementSystem.Models;
using LearningManagementSystem.Models.ViewModels;
using LearningManagementSystem.Repositories;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LearningManagementSystem.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LearningManagementSystem.Utilities;
using LearningManagementSystem.Services;

namespace LearningManagementSystem.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ICartItemRepository _cartItemRepository;
        private readonly LMSContext _context;
        private readonly ILogger<CartController> _logger;
        private readonly IRevenueShareRepository _revenueShareRepository;
        private readonly IUserRepository _userRepository;
        private readonly EmailService _emailService;

        public CartController(
            ICartItemRepository cartItemRepository,
            LMSContext context,
            ILogger<CartController> logger,
            IRevenueShareRepository revenueShareRepository,
            IUserRepository userRepository,
            EmailService emailService)
        {
            _cartItemRepository = cartItemRepository;
            _context = context;
            _logger = logger;
            _revenueShareRepository = revenueShareRepository;
            _userRepository = userRepository;
            _emailService = emailService;
        }

        private async Task<Cart> GetOrCreateCartAsync(string userName)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserName == userName && c.Status == "Active");

            if (cart == null)
            {
                cart = new Cart
                {
                    CartId = Guid.NewGuid().ToString(),
                    UserName = userName,
                    CreatedDate = DateTime.Now,
                    LastModifiedDate = DateTime.Now,
                    TotalAmount = 0,
                    Status = "Active",
                    CartItems = new List<CartItem>()
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            return cart;
        }

        public async Task<IActionResult> Index()
        {
            var userName = User.Identity.Name;
            var cart = await GetOrCreateCartAsync(userName);
            var cartItems = cart.CartItems;

            var cartItemViewModels = new List<CartItemViewModel>();
            foreach (var item in cartItems)
            {
                string instructorName = "Unknown Instructor";
                double? averageRating = null;

                var course = await _context.Courses
                    .Include(c => c.CourseInstructors)
                        .ThenInclude(ci => ci.User)
                            .ThenInclude(u => u.Role) // Đảm bảo nạp Role
                    .Include(c => c.Comments)
                    .Include(c => c.Lessons)
                    .Include(c => c.Assignments)
                    .Include(c => c.CourseTags)
                        .ThenInclude(ct => ct.Tag)
                    .FirstOrDefaultAsync(c => c.CourseId == item.CourseId);

                CourseListViewModel courseViewModel = null;
                if (course != null)
                {
                    if (course.CourseInstructors?.Any() == true)
                    {
                        var instructor = course.CourseInstructors
                            .FirstOrDefault(ci => ci.User?.Role?.RoleName == "Instructor")?.User;
                        if (instructor != null)
                        {
                            instructorName = instructor.FullName ?? "Unknown Instructor";
                        }
                    }

                    if (course.Comments?.Any() == true)
                    {
                        averageRating = (double?)course.Comments.Average(cm => cm.Rating);
                    }

                    courseViewModel = new CourseListViewModel
                    {
                        CourseId = course.CourseId.ToString(),
                        CourseName = course.CourseName,
                        Description = course.Description,
                        CreatedDate = course.CreatedDate,
                        ImageUrl = string.IsNullOrEmpty(course.ImageUrl) ? "/images/default-course.jpg" : course.ImageUrl,
                        IsEnrolled = false,
                        Price = course.Price,
                        Title = course.CourseName,
                        Lessons = course.Lessons?.ToList(),
                        Assignments = course.Assignments?.ToList(),
                        AverageRating = averageRating,
                        Level = course.Level,
                        DurationMinutes = course.DurationMinutes,
                        TagNames = course.CourseTags?
                            .Select(ct => ct.Tag != null ? ct.Tag.Name : string.Empty)
                            .Where(name => !string.IsNullOrWhiteSpace(name))
                            .ToList() ?? new List<string>()
                    };
                }

                cartItemViewModels.Add(new CartItemViewModel
                {
                    CartItem = item,
                    InstructorName = instructorName,
                    Course = courseViewModel
                });
            }

            var cartCourseIds = cartItems.Select(ci => ci.CourseId).ToList();
            var recommendedCourses = await _context.Courses
                .Include(c => c.Comments)
                .Include(c => c.Lessons)
                .Include(c => c.Assignments)
                .Include(c => c.CourseTags)
                    .ThenInclude(ct => ct.Tag)
                .Where(c => !cartCourseIds.Contains(c.CourseId))
                .OrderBy(c => c.CreatedDate)
                .Take(4)
                .ToListAsync();

            if (recommendedCourses.Count < 4)
            {
                var additionalCourses = await _context.Courses
                    .Include(c => c.Comments)
                    .Include(c => c.Lessons)
                    .Include(c => c.Assignments)
                    .Include(c => c.CourseTags)
                        .ThenInclude(ct => ct.Tag)
                    .Where(c => !cartCourseIds.Contains(c.CourseId) && !recommendedCourses.Select(rc => rc.CourseId).Contains(c.CourseId))
                    .OrderBy(c => c.CreatedDate)
                    .Take(4 - recommendedCourses.Count)
                    .ToListAsync();
                recommendedCourses.AddRange(additionalCourses);
            }

            var recommendedViewModels = new List<CourseListViewModel>();
            foreach (var course in recommendedCourses ?? new List<Course>())
            {
                double? avgRating = null;
                if (course.Comments?.Any() == true)
                {
                    avgRating = (double?)course.Comments.Average(cm => cm.Rating);
                }

                recommendedViewModels.Add(new CourseListViewModel
                {
                    CourseId = course.CourseId.ToString(),
                    CourseName = course.CourseName,
                    Description = course.Description,
                    CreatedDate = course.CreatedDate,
                    ImageUrl = string.IsNullOrEmpty(course.ImageUrl) ? "/images/default-course.jpg" : course.ImageUrl,
                    IsEnrolled = false,
                    Price = course.Price,
                    Title = course.CourseName,
                    Lessons = course.Lessons?.ToList(),
                    Assignments = course.Assignments?.ToList(),
                    AverageRating = avgRating,
                    Level = course.Level,
                    DurationMinutes = course.DurationMinutes,
                    TagNames = course.CourseTags?
                        .Select(ct => ct.Tag != null ? ct.Tag.Name : string.Empty)
                        .Where(name => !string.IsNullOrWhiteSpace(name))
                        .ToList() ?? new List<string>()
                });
            }

            ViewBag.RecommendedCourses = recommendedViewModels ?? new List<CourseListViewModel>();
            ViewBag.CartId = cart.CartId;

            return View(cartItemViewModels);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(string courseId)
        {
            if (string.IsNullOrEmpty(courseId))
            {
                TempData["Error"] = "Khóa học không hợp lệ.";
                return RedirectToAction("Index", "Search");
            }

            var userName = User.Identity.Name;
            var cart = await GetOrCreateCartAsync(userName);

            var existingCartItem = cart.CartItems.FirstOrDefault(ci => ci.CourseId == courseId);
            if (existingCartItem != null)
            {
                TempData["Error"] = "Khóa học đã có trong giỏ hàng.";
                return RedirectToAction("Index", "Home");
            }

            var course = await _context.Courses.FindAsync(courseId);
            if (course == null)
            {
                TempData["Error"] = "Khóa học không tồn tại.";
                return RedirectToAction("Index", "Search");
            }

            // Business Rule: Kiểm tra user đã đăng ký khóa học chưa
            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.UserName == userName && e.CourseId == courseId);
            if (enrollment != null)
            {
                TempData["Error"] = "Bạn đã đăng ký khóa học này rồi. Không thể thêm vào giỏ hàng.";
                return RedirectToAction("Index");
            }

            // Business Rule: Giá tiền không được âm
            var coursePrice = course.Price ?? 0m;
            if (coursePrice < 0)
            {
                _logger.LogWarning($"Course {courseId} has negative price: {coursePrice}. Setting to 0.");
                coursePrice = 0m;
            }

            var cartItem = new CartItem
            {
                CartItemId = Guid.NewGuid().ToString(),
                CartId = cart.CartId,
                CourseId = courseId,
                AddedDate = DateTime.Now,
                Price = coursePrice
            };

            cart.CartItems.Add(cartItem);
            cart.TotalAmount += cartItem.Price;  // Update TotalAmount with the non-null price
            cart.LastModifiedDate = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Thêm khóa học vào giỏ hàng thành công!";
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromCart(string cartItemId)
        {
            if (string.IsNullOrEmpty(cartItemId))
            {
                TempData["Error"] = "Mục giỏ hàng không hợp lệ.";
                return RedirectToAction("Index");
            }

            var cartItem = await _context.CartItems
                .Include(ci => ci.Cart)
                .FirstOrDefaultAsync(ci => ci.CartItemId == cartItemId && ci.Cart.UserName == User.Identity.Name);

            if (cartItem == null)
            {
                TempData["Error"] = "Mục giỏ hàng không tồn tại hoặc không thuộc về bạn.";
                return RedirectToAction("Index");
            }

            var cart = cartItem.Cart;
            cart.TotalAmount -= cartItem.Price;
            cart.LastModifiedDate = DateTime.Now;

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã xóa khóa học khỏi giỏ hàng.";
            return RedirectToAction("Index");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(string selectedCartItemIds)
        {
            // Debug log
            _logger.LogInformation($"Received selectedCartItemIds: {selectedCartItemIds}");

            // Parse the comma-separated string into array
            string[] selectedIds = null;
            if (!string.IsNullOrEmpty(selectedCartItemIds))
            {
                selectedIds = selectedCartItemIds.Split(',', StringSplitOptions.RemoveEmptyEntries);
            }

            if (selectedIds == null || !selectedIds.Any())
            {
                TempData["Error"] = "Vui lòng chọn ít nhất một khóa học để thanh toán.";
                return RedirectToAction("Index");
            }

            var userName = User.Identity.Name;
            if (string.IsNullOrEmpty(userName))
            {
                return RedirectToAction("Login", "Account");
            }

            // Lấy giỏ hàng của người dùng
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Course)
                .FirstOrDefaultAsync(c => c.UserName == userName);

            if (cart == null || !cart.CartItems.Any())
            {
                TempData["Error"] = "Giỏ hàng của bạn hiện đang trống.";
                return RedirectToAction("Index");
            }

            // Lọc các CartItem theo selectedCartItemIds
            var cartItems = cart.CartItems
                .Where(ci => selectedIds.Contains(ci.CartItemId))
                .ToList();

            _logger.LogInformation($"Found {cartItems.Count} cart items from selected IDs");

            if (!cartItems.Any())
            {
                TempData["Error"] = "Không tìm thấy các khóa học đã chọn.";
                return RedirectToAction("Index");
            }

            // Business Rule: Kiểm tra user không thể mua khóa học đã đăng ký
            var courseIds = cartItems.Select(ci => ci.CourseId).ToList();
            var existingEnrollments = await _context.Enrollments
                .Where(e => e.UserName == userName && courseIds.Contains(e.CourseId))
                .Select(e => e.CourseId)
                .ToListAsync();

            if (existingEnrollments.Any())
            {
                var enrolledCourseNames = await _context.Courses
                    .Where(c => existingEnrollments.Contains(c.CourseId))
                    .Select(c => c.CourseName)
                    .ToListAsync();
                
                TempData["Error"] = $"Bạn đã đăng ký các khóa học sau: {string.Join(", ", enrolledCourseNames)}. Vui lòng xóa chúng khỏi giỏ hàng.";
                return RedirectToAction("Index");
            }

            // Xây dựng danh sách CartItemViewModel
            var cartItemViewModels = new List<CartItemViewModel>();
            foreach (var item in cartItems)
            {
                string instructorName = "Unknown Instructor";
                double? averageRating = null;

                // Lấy thông tin khóa học và giảng viên
                var course = await _context.Courses
                    .Include(c => c.CourseInstructors)
                        .ThenInclude(ci => ci.User)
                            .ThenInclude(u => u.Role)
                    .Include(c => c.Comments)
                    .Include(c => c.Lessons)
                    .Include(c => c.Assignments)
                    .Include(c => c.CourseTags)
                        .ThenInclude(ct => ct.Tag)
                    .FirstOrDefaultAsync(c => c.CourseId == item.CourseId);

                CourseListViewModel courseViewModel = null;
                if (course != null)
                {
                    // Lấy tên giảng viên
                    if (course.CourseInstructors?.Any() == true)
                    {
                        var instructor = course.CourseInstructors
                            .FirstOrDefault(ci => ci.User?.Role?.RoleName == "Instructor")?.User;
                        if (instructor != null)
                        {
                            instructorName = instructor.FullName ?? "Unknown Instructor";
                            _logger.LogInformation($"Found instructor for course {course.CourseId}: {instructorName}");
                        }
                        else
                        {
                            _logger.LogWarning($"No instructor found for course {course.CourseId} with role 'Instructor'");
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"No CourseInstructors found for course {course.CourseId}");
                    }

                    // Tính điểm đánh giá trung bình
                    if (course.Comments?.Any() == true)
                    {
                        averageRating = (double?)course.Comments.Average(cm => cm.Rating);
                    }

                    // Tạo CourseListViewModel
                    courseViewModel = new CourseListViewModel
                    {
                        CourseId = course.CourseId.ToString(),
                        CourseName = course.CourseName,
                        Description = course.Description,
                        CreatedDate = course.CreatedDate,
                        ImageUrl = string.IsNullOrEmpty(course.ImageUrl) ? "/images/default-course.jpg" : course.ImageUrl,
                        IsEnrolled = false,
                        Price = course.Price,
                        Title = course.CourseName,
                        Lessons = course.Lessons?.ToList(),
                        Assignments = course.Assignments?.ToList(),
                        AverageRating = averageRating,
                        Level = course.Level,
                        DurationMinutes = course.DurationMinutes,
                        TagNames = course.CourseTags?
                            .Select(ct => ct.Tag != null ? ct.Tag.Name : string.Empty)
                            .Where(name => !string.IsNullOrWhiteSpace(name))
                            .ToList() ?? new List<string>()
                    };

                    // Gán lại Course cho CartItem (để view có thể truy cập)
                    item.Course = course;
                }

                cartItemViewModels.Add(new CartItemViewModel
                {
                    CartItem = item,
                    InstructorName = instructorName,
                    Course = courseViewModel
                });
            }

            return View("Checkout", cartItemViewModels);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InitiateVnpayPayment(decimal amount, string selectedCartItemIds)
        {
            // Business Rule: Kiểm tra amount không được âm
            if (amount < 0)
            {
                _logger.LogWarning($"Invalid payment amount: {amount}. Amount cannot be negative.");
                TempData["Error"] = "Số tiền thanh toán không hợp lệ.";
                return RedirectToAction("Checkout");
            }

            var username = User.Identity.Name;
            var cart = await GetOrCreateCartAsync(username);

            var existingPendingPayment = _context.Payments
                .FirstOrDefault(p => p.UserName == username && p.PaymentStatus == "Pending" && p.PaymentMethod == "VNPay");

            // Parse selected cart item IDs
            var selectedIds = selectedCartItemIds?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? new string[0];
            var cartItems = cart.CartItems
                .Where(ci => selectedIds.Contains(ci.CartItemId))
                .ToList();

            if (!cartItems.Any())
            {
                TempData["Error"] = "Không có khóa học nào được chọn để thanh toán.";
                return RedirectToAction("Checkout");
            }

            // Business Rule: Kiểm tra user không thể mua khóa học đã đăng ký
            var courseIds = cartItems.Select(ci => ci.CourseId).ToList();
            var existingEnrollments = await _context.Enrollments
                .Where(e => e.UserName == username && courseIds.Contains(e.CourseId))
                .Select(e => e.CourseId)
                .ToListAsync();

            if (existingEnrollments.Any())
            {
                var enrolledCourseNames = await _context.Courses
                    .Where(c => existingEnrollments.Contains(c.CourseId))
                    .Select(c => c.CourseName)
                    .ToListAsync();
                
                TempData["Error"] = $"Bạn đã đăng ký các khóa học sau: {string.Join(", ", enrolledCourseNames)}. Không thể thanh toán.";
                return RedirectToAction("Index");
            }

            // Business Rule: Tính tổng tiền từ cart items và so sánh với amount từ client
            var calculatedTotal = cartItems.Sum(ci => ci.Price);
            
            // Business Rule: Đảm bảo tất cả giá tiền không âm
            if (cartItems.Any(ci => ci.Price < 0))
            {
                _logger.LogError("Found cart items with negative prices. This should not happen.");
                TempData["Error"] = "Có lỗi xảy ra với giá tiền. Vui lòng liên hệ hỗ trợ.";
                return RedirectToAction("Index");
            }

            // Cho phép sai số nhỏ do làm tròn (0.01 VND)
            if (Math.Abs(calculatedTotal - amount) > 0.01m)
            {
                _logger.LogWarning($"Payment amount mismatch. Calculated: {calculatedTotal}, Received: {amount}");
                TempData["Error"] = "Số tiền thanh toán không khớp với tổng giá trị giỏ hàng. Vui lòng thử lại.";
                return RedirectToAction("Checkout");
            }

            string paymentId;
            
            // Kiểm tra và xử lý existing pending payment
            if (existingPendingPayment != null)
            {
                // Business Rule: Kiểm tra amount của existing payment có khớp không
                if (Math.Abs(existingPendingPayment.Amount - calculatedTotal) > 0.01m)
                {
                    _logger.LogWarning($"Existing payment amount mismatch. Payment: {existingPendingPayment.Amount}, Calculated: {calculatedTotal}");
                    // Xóa payment cũ và tạo mới
                    _context.Payments.Remove(existingPendingPayment);
                    await _context.SaveChangesAsync();
                    existingPendingPayment = null;
                }
            }

            // Tạo payment mới nếu không có existing payment hoặc đã xóa payment cũ
            if (existingPendingPayment == null)
            {
                string orderId = DateTime.Now.Ticks.ToString();
                paymentId = $"PAY_{orderId}";

                var payment = new Payment
                {
                    PaymentId = paymentId,
                    UserName = username,
                    Amount = calculatedTotal, // Sử dụng calculatedTotal thay vì amount từ client để đảm bảo an toàn
                    PaymentDate = DateTime.Now,
                    PaymentStatus = "Pending",
                    PaymentMethod = "VNPay",
                    TransactionType = "CoursePurchase"
                };

                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                foreach (var item in cartItems)
                {
                    var orderDetail = new OrderDetail
                    {
                        OrderDetailId = Guid.NewGuid().ToString(),
                        PaymentId = paymentId,
                        CourseId = item.CourseId,
                        Price = item.Price
                    };
                    _context.OrderDetails.Add(orderDetail);
                }
                await _context.SaveChangesAsync();
            }
            else
            {
                // Sử dụng existing payment
                paymentId = existingPendingPayment.PaymentId;
            }

            var vnpay = new VnPayLibrary();
            var vnpayRequestData = new SortedList<string, string>();

            vnpayRequestData.Add("vnp_Version", VnPayConfig.vnp_Version);
            vnpayRequestData.Add("vnp_Command", VnPayConfig.vnp_Command);
            vnpayRequestData.Add("vnp_TmnCode", VnPayConfig.vnp_TmnCode);
            
            // Business Rule: Sử dụng calculatedTotal thay vì amount từ client để đảm bảo an toàn
            var finalAmount = existingPendingPayment != null 
                ? existingPendingPayment.Amount 
                : calculatedTotal;
            
            // VNPay yêu cầu amount phải là số nguyên (đã nhân 100), không có dấu chấm thập phân
            long amountInCents = (long)(finalAmount * 100);
            vnpayRequestData.Add("vnp_Amount", amountInCents.ToString());
            
            // Format date theo yêu cầu VNPay: yyyyMMddHHmmss
            vnpayRequestData.Add("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnpayRequestData.Add("vnp_CurrCode", VnPayConfig.vnp_CurrCode);
            
            // IP Address - đảm bảo format đúng
            string ipAddress = VnPayLibrary.GetIpAddress(HttpContext);
            vnpayRequestData.Add("vnp_IpAddr", ipAddress);
            
            vnpayRequestData.Add("vnp_Locale", VnPayConfig.vnp_Locale);
            
            // OrderInfo - loại bỏ ký tự đặc biệt và giới hạn độ dài
            string orderInfo = $"Thanh toan khoa hoc - {username}";
            // VNPay yêu cầu OrderInfo không quá 255 ký tự và không có ký tự đặc biệt
            if (orderInfo.Length > 255)
            {
                orderInfo = orderInfo.Substring(0, 255);
            }
            vnpayRequestData.Add("vnp_OrderInfo", orderInfo);
            
            vnpayRequestData.Add("vnp_OrderType", "250000");
            
            // ReturnUrl - đảm bảo URL hợp lệ
            string returnUrl = Url.Action("VnPayReturn", "Cart", null, Request.Scheme) ?? VnPayConfig.vnp_Returnurl;
            if (string.IsNullOrEmpty(returnUrl))
            {
                returnUrl = VnPayConfig.vnp_Returnurl;
            }
            vnpayRequestData.Add("vnp_ReturnUrl", returnUrl);
            
            // TxnRef - đảm bảo không có ký tự đặc biệt và độ dài hợp lệ
            if (string.IsNullOrEmpty(paymentId) || paymentId.Length > 100)
            {
                _logger.LogError($"Invalid paymentId: {paymentId}");
                TempData["Error"] = "Mã giao dịch không hợp lệ.";
                return RedirectToAction("Checkout");
            }
            vnpayRequestData.Add("vnp_TxnRef", paymentId);

            string paymentUrl = VnPayLibrary.CreateRequestUrl(VnPayConfig.vnp_Url, vnpayRequestData, VnPayConfig.vnp_HashSecret);

            return Redirect(paymentUrl);
        }

        [AllowAnonymous]
        public async Task<IActionResult> VnPayReturn()
        {
            SortedList<string, string> responseData = new SortedList<string, string>();
            foreach (var key in Request.Query.Keys)
            {
                if (!key.StartsWith("vnp_")) continue;

                string value = Request.Query[key];
                responseData.Add(key, value);
            }

            string vnp_SecureHash = Request.Query["vnp_SecureHash"];
            responseData.Remove("vnp_SecureHash");

            bool isValidSignature = VnPayLibrary.ValidateSignature(vnp_SecureHash, VnPayConfig.vnp_HashSecret, responseData);

            if (!isValidSignature)
            {
                _logger.LogWarning("VNPay signature validation failed.");
                TempData["Error"] = "Xác thực thanh toán thất bại.";
                return RedirectToAction("Checkout");
            }

            string paymentId = Request.Query["vnp_TxnRef"];
            string vnPayTransactionId = Request.Query["vnp_TransactionNo"];
            string responseCode = Request.Query["vnp_ResponseCode"];

            var payment = await _context.Payments.FirstOrDefaultAsync(p => p.PaymentId == paymentId);

            if (payment == null)
            {
                _logger.LogWarning($"Payment with id {paymentId} not found.");
                TempData["Error"] = "Không tìm thấy thông tin thanh toán.";
                return RedirectToAction("Checkout");
            }

            if (responseCode == "00")
            {
                // Business Rule: Sử dụng transaction để đảm bảo data consistency
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    payment.PaymentStatus = "Completed";

                    var username = payment.UserName;

                    var orderDetails = await _context.OrderDetails
                        .Where(od => od.PaymentId == paymentId)
                        .ToListAsync();

                    var cart = await _context.Carts
                        .Include(c => c.CartItems)
                        .FirstOrDefaultAsync(c => c.UserName == username && c.Status == "Active");

                    foreach (var detail in orderDetails)
                    {
                        // Business Rule: Kiểm tra user đã đăng ký khóa học này chưa
                        var existingEnrollment = await _context.Enrollments
                            .FirstOrDefaultAsync(e => e.UserName == username && e.CourseId == detail.CourseId);

                        if (existingEnrollment == null)
                        {
                            var enrollment = new Enrollment
                            {
                                EnrollmentId = Guid.NewGuid().ToString(),
                                UserName = username,
                                CourseId = detail.CourseId,
                                EnrollmentDate = DateTime.Now,
                            };

                            _context.Enrollments.Add(enrollment);
                        }

                        // Chia doanh thu cho giảng viên và admin
                        try
                        {
                            await ProcessRevenueShare(paymentId, detail.CourseId, detail.Price);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Error processing revenue share for PaymentId: {paymentId}, CourseId: {detail.CourseId}");
                            // Không throw exception để không rollback toàn bộ transaction
                            // Revenue share có thể được xử lý sau
                        }
                    }

                    if (cart != null)
                    {
                        var selectedCourseIds = orderDetails.Select(od => od.CourseId).ToList();
                        var itemsToRemove = cart.CartItems.Where(ci => selectedCourseIds.Contains(ci.CourseId)).ToList();
                        cart.TotalAmount = cart.CartItems.Except(itemsToRemove).Sum(ci => ci.Price);
                        cart.LastModifiedDate = DateTime.Now;
                        _context.CartItems.RemoveRange(itemsToRemove);

                        if (!cart.CartItems.Any())
                        {
                            cart.Status = "Inactive";
                        }
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // Gửi email invoice sau khi transaction commit thành công
                    try
                    {
                        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == username);
                        if (user != null && !string.IsNullOrEmpty(user.Email))
                        {
                            var emailBody = GenerateInvoiceEmail(payment, orderDetails, user);
                            await _emailService.SendEmailAsync(
                                user.Email,
                                $"Hóa đơn thanh toán #{payment.PaymentId} - E-Learning System",
                                emailBody
                            );
                            _logger.LogInformation($"Invoice email sent successfully to {user.Email} for payment {paymentId}.");
                        }
                    }
                    catch (Exception ex)
                    {
                        // Email failure không nên ảnh hưởng đến payment success
                        _logger.LogError(ex, $"Error sending invoice email for PaymentId: {paymentId}");
                    }

                    _logger.LogInformation($"Payment {paymentId} completed successfully. Redirecting to PaymentSuccess.");
                    return RedirectToAction("PaymentSuccess", new { id = paymentId });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, $"Error processing payment completion for PaymentId: {paymentId}. Transaction rolled back.");
                    TempData["Error"] = "Đã có lỗi xảy ra khi xử lý thanh toán. Vui lòng liên hệ hỗ trợ.";
                    return RedirectToAction("Checkout");
                }
            }
            else if (responseCode == "24") // 24: User cancel
            {
                payment.PaymentStatus = "Failed";
                await _context.SaveChangesAsync();
                _logger.LogWarning($"Payment {paymentId} was cancelled by user (response code 24).");
                TempData["Error"] = "Bạn đã hủy giao dịch thanh toán.";

                var orderDetails = await _context.OrderDetails
                    .Where(od => od.PaymentId == paymentId)
                    .ToListAsync();

                var userName = payment.UserName;
                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.UserName == userName && c.Status == "Active");

                if (cart != null)
                {
                    var selectedCourseIds = orderDetails.Select(od => od.CourseId).ToList();
                    var cartItems = cart.CartItems
                        .Where(ci => selectedCourseIds.Contains(ci.CourseId))
                        .ToList();
                    var selectedCartItemIds = string.Join(",", cartItems.Select(ci => ci.CartItemId));
                    TempData["SelectedCartItemIds"] = selectedCartItemIds;
                }

                return RedirectToAction("Checkout");
            }
            else
            {
                payment.PaymentStatus = "Failed";
                await _context.SaveChangesAsync();
                _logger.LogWarning($"Payment {paymentId} failed with response code {responseCode}.");
                TempData["Error"] = "Thanh toán thất bại. Vui lòng thử lại sau.";
                return RedirectToAction("Checkout");
            }
        }

        public async Task<IActionResult> PaymentSuccess(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                _logger.LogWarning("PaymentSuccess called with null or empty id.");
                return NotFound();
            }

            var username = User.Identity.Name;

            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.PaymentId == id && p.UserName == username);

            if (payment == null)
            {
                _logger.LogWarning($"Payment with id {id} not found for user {username}.");
                return NotFound();
            }

            if (payment.PaymentStatus != "Completed")
            {
                _logger.LogWarning($"Payment {id} has status {payment.PaymentStatus}, expected 'Completed'.");
                return BadRequest("Giao dịch chưa hoàn tất hoặc không hợp lệ.");
            }

            var orderDetails = await _context.OrderDetails
                .Include(od => od.Course)
                .Where(od => od.PaymentId == id)
                .ToListAsync();

            // Lấy thông tin user để gửi email
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == username);

            // Gửi email hóa đơn nếu user có email
            if (user != null && !string.IsNullOrEmpty(user.Email))
            {
                try
                {
                    var emailBody = GenerateInvoiceEmail(payment, orderDetails, user);
                    await _emailService.SendEmailAsync(
                        user.Email,
                        $"Hóa đơn thanh toán #{payment.PaymentId} - E-Learning System",
                        emailBody
                    );
                    _logger.LogInformation($"Invoice email sent successfully to {user.Email} for payment {id}.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to send invoice email to {user.Email} for payment {id}.");
                    // Không throw exception để không ảnh hưởng đến trải nghiệm người dùng
                }
            }

            var viewModel = new PaymentDetailViewModel
            {
                Payment = payment,
                OrderDetails = orderDetails,
                SuccessMessage = "Thanh toán của bạn đã được thực hiện thành công! Cảm ơn bạn đã sử dụng dịch vụ của chúng tôi."
            };

            _logger.LogInformation($"PaymentSuccess view rendered for payment {id}.");
            return View(viewModel);
        }

        private string GenerateInvoiceEmail(Payment payment, List<OrderDetail> orderDetails, User user)
        {
            var emailHtml = $@"
<!DOCTYPE html>
<html lang='vi'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Hóa đơn thanh toán</title>
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            line-height: 1.6;
            color: #333;
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
            background-color: #f5f5f5;
        }}
        .invoice-container {{
            background: #ffffff;
            border-radius: 12px;
            padding: 30px;
            box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
        }}
        .header {{
            text-align: center;
            border-bottom: 3px solid #7c3aed;
            padding-bottom: 20px;
            margin-bottom: 30px;
        }}
        .header h1 {{
            color: #7c3aed;
            margin: 0;
            font-size: 28px;
        }}
        .header p {{
            color: #666;
            margin: 5px 0;
        }}
        .invoice-info {{
            display: flex;
            justify-content: space-between;
            margin-bottom: 30px;
            padding: 20px;
            background: #f8f9fa;
            border-radius: 8px;
        }}
        .info-section {{
            flex: 1;
        }}
        .info-section h3 {{
            color: #7c3aed;
            margin-top: 0;
            font-size: 16px;
            margin-bottom: 10px;
        }}
        .info-section p {{
            margin: 5px 0;
            color: #555;
        }}
        .course-table {{
            width: 100%;
            border-collapse: collapse;
            margin-bottom: 20px;
        }}
        .course-table th {{
            background: #7c3aed;
            color: white;
            padding: 12px;
            text-align: left;
            font-weight: 600;
        }}
        .course-table td {{
            padding: 12px;
            border-bottom: 1px solid #e0e0e0;
        }}
        .course-table tr:hover {{
            background: #f8f9fa;
        }}
        .total-section {{
            background: linear-gradient(135deg, #7c3aed 0%, #6366f1 100%);
            color: white;
            padding: 20px;
            border-radius: 8px;
            text-align: right;
            margin-top: 20px;
        }}
        .total-section .label {{
            font-size: 18px;
            margin-bottom: 10px;
        }}
        .total-section .amount {{
            font-size: 32px;
            font-weight: bold;
        }}
        .footer {{
            text-align: center;
            margin-top: 30px;
            padding-top: 20px;
            border-top: 1px solid #e0e0e0;
            color: #666;
            font-size: 14px;
        }}
        .success-badge {{
            display: inline-block;
            background: #10b981;
            color: white;
            padding: 8px 16px;
            border-radius: 20px;
            font-weight: 600;
            margin-bottom: 20px;
        }}
    </style>
</head>
<body>
    <div class='invoice-container'>
        <div class='header'>
            <h1>🎓 E-Learning System</h1>
            <p>Hóa đơn thanh toán</p>
            <span class='success-badge'>✓ Thanh toán thành công</span>
        </div>

        <div class='invoice-info'>
            <div class='info-section'>
                <h3>Thông tin khách hàng</h3>
                <p><strong>Họ tên:</strong> {user.FullName ?? user.UserName}</p>
                <p><strong>Email:</strong> {user.Email}</p>
                <p><strong>Mã giao dịch:</strong> #{payment.PaymentId}</p>
            </div>
            <div class='info-section'>
                <h3>Thông tin thanh toán</h3>
                <p><strong>Ngày thanh toán:</strong> {payment.PaymentDate:dd/MM/yyyy HH:mm}</p>
                <p><strong>Phương thức:</strong> {payment.PaymentMethod ?? "VNPay"}</p>
                <p><strong>Trạng thái:</strong> {payment.PaymentStatus}</p>
            </div>
        </div>

        <h3 style='color: #7c3aed; margin-bottom: 15px;'>Khóa học đã mua:</h3>
        <table class='course-table'>
            <thead>
                <tr>
                    <th>STT</th>
                    <th>Tên khóa học</th>
                    <th style='text-align: right;'>Giá tiền</th>
                </tr>
            </thead>
            <tbody>";

            int index = 1;
            foreach (var item in orderDetails)
            {
                emailHtml += $@"
                <tr>
                    <td>{index}</td>
                    <td>{item.Course?.CourseName ?? "N/A"}</td>
                    <td style='text-align: right;'>{item.Price:N0}₫</td>
                </tr>";
                index++;
            }

            emailHtml += $@"
            </tbody>
        </table>

        <div class='total-section'>
            <div class='label'>Tổng số tiền:</div>
            <div class='amount'>{payment.Amount:N0}₫</div>
        </div>

        <div class='footer'>
            <p>Cảm ơn bạn đã sử dụng dịch vụ của chúng tôi!</p>
            <p>Nếu có thắc mắc, vui lòng liên hệ với chúng tôi qua hotline hoặc email hỗ trợ.</p>
            <p style='margin-top: 20px; color: #999;'>© {DateTime.Now.Year} E-Learning System. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

            return emailHtml;
        }

        private async Task ProcessRevenueShare(string paymentId, string courseId, decimal coursePrice)
        {
            try
            {
                _logger.LogInformation($"Processing revenue share for PaymentId: {paymentId}, CourseId: {courseId}, Price: {coursePrice}");

                // Lấy danh sách giảng viên của khóa học
                var instructors = await _context.CourseInstructors
                    .Where(ci => ci.CourseId == courseId)
                    .ToListAsync();

                _logger.LogInformation($"Found {instructors.Count} instructors for course {courseId}");

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
                        _logger.LogInformation($"Created revenue share for instructor {instructor.UserName}: {instructorAmount} VNĐ");

                        // Cộng tiền vào ví giảng viên
                        var instructorUser = _userRepository.GetByUserName(instructor.UserName);
                        if (instructorUser != null)
                        {
                            instructorUser.WalletBalance += instructorAmount;
                            _userRepository.Update(instructorUser);
                            _logger.LogInformation($"Updated wallet balance for {instructor.UserName}: {instructorUser.WalletBalance} VNĐ");
                        }
                        else
                        {
                            _logger.LogWarning($"Instructor user not found: {instructor.UserName}");
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
                _logger.LogInformation($"Created revenue share for admin: {adminAmount} VNĐ");

                // Cộng tiền vào ví admin
                var adminUser = _userRepository.GetByUserName("admin");
                if (adminUser != null)
                {
                    adminUser.WalletBalance += adminAmount;
                    _userRepository.Update(adminUser);
                    _logger.LogInformation($"Updated wallet balance for admin: {adminUser.WalletBalance} VNĐ");
                }
                else
                {
                    _logger.LogWarning("Admin user not found");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in ProcessRevenueShare for PaymentId: {paymentId}, CourseId: {courseId}");
                throw; // Re-throw để caller có thể xử lý
            }
        }
    }
}