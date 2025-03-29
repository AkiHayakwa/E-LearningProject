using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LearningManagementSystem.Data;
using LearningManagementSystem.Models;
using LearningManagementSystem.Repositories;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using LearningManagementSystem.Models.ViewModels;

namespace LearningManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly LMSContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly ILogger<AccountController> _logger;
        private readonly IEnrollmentRepository _enrollmentRepository;

        public AccountController(
            LMSContext context,
            IPasswordHasher<User> passwordHasher,
            ILogger<AccountController> logger,
            IEnrollmentRepository enrollmentRepository)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _logger = logger;
            _enrollmentRepository = enrollmentRepository;
        }

        #region Đăng nhập (Login)

        // GET: Account/Login
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string userName, string password, string returnUrl = null)
        {
            _logger.LogInformation($"Login attempt for UserName: {userName}");

            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
            {
                _logger.LogWarning("UserName or Password is empty.");
                ViewBag.Error = "Vui lòng nhập tên đăng nhập và mật khẩu.";
                return View();
            }

            // Tải quan hệ Role cùng với User
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserName == userName);

            if (user == null)
            {
                _logger.LogWarning($"User not found for UserName: {userName}");
                ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không đúng.";
                return View();
            }

            // Kiểm tra mật khẩu
            var result = _passwordHasher.VerifyHashedPassword(user, user.Password, password);
            if (result != PasswordVerificationResult.Success)
            {
                _logger.LogWarning($"Invalid password for UserName: {userName}");
                ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không đúng.";
                return View();
            }

            // Kiểm tra user.Role có null không
            if (user.Role == null)
            {
                _logger.LogError($"Role not found for UserName: {userName}");
                ViewBag.Error = "Không tìm thấy vai trò của người dùng. Vui lòng liên hệ quản trị viên.";
                return View();
            }

            // Kiểm tra trạng thái tài khoản (nếu có thuộc tính IsActive)
            // if (!user.IsActive) // Uncomment nếu model User có thuộc tính IsActive
            // {
            //     _logger.LogWarning($"Account is inactive for UserName: {userName}");
            //     ViewBag.Error = "Tài khoản của bạn đã bị khóa. Vui lòng liên hệ quản trị viên.";
            //     return View();
            // }

            // Tạo claims cho người dùng
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Role, user.Role.RoleName)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true // Giữ đăng nhập sau khi đóng trình duyệt
            };

            // Đăng nhập người dùng
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            _logger.LogInformation($"User {userName} logged in successfully. Role: {user.Role.RoleName}");

            // Chuyển hướng dựa trên vai trò
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            if (user.Role.RoleName == "Admin")
            {
                return RedirectToAction("Dashboard", "Admin", new { area = "" });
            }

            return RedirectToAction("Index", "Home");
        }

        #endregion

        #region Đăng ký (Register)

        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(User model, string password, string confirmPassword)
        {
            _logger.LogInformation($"Register attempt for UserName: {model.UserName}");

            // Kiểm tra mật khẩu và xác nhận mật khẩu
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword) || password != confirmPassword)
            {
                _logger.LogWarning("Password and ConfirmPassword do not match.");
                ModelState.AddModelError("ConfirmPassword", "Mật khẩu xác nhận không khớp.");
            }

            // Tự động gán vai trò "Student"
            var studentRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Student");
            if (studentRole == null)
            {
                _logger.LogError("Role 'Student' not found in the system.");
                ModelState.AddModelError("", "Vai trò 'Student' không tồn tại trong hệ thống. Vui lòng liên hệ quản trị viên.");
                return View(model);
            }

            model.RoleId = studentRole.RoleId;

            // Xóa lỗi validation của RoleId trong ModelState (nếu có)
            if (ModelState.ContainsKey("RoleId"))
            {
                ModelState.Remove("RoleId");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra xem tên đăng nhập đã tồn tại chưa
                    var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.UserName == model.UserName);
                    if (existingUser != null)
                    {
                        _logger.LogWarning($"UserName {model.UserName} already exists.");
                        ModelState.AddModelError("UserName", "Tên đăng nhập đã tồn tại.");
                        return View(model);
                    }

                    // Kiểm tra email đã tồn tại chưa
                    var existingEmail = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
                    if (existingEmail != null)
                    {
                        _logger.LogWarning($"Email {model.Email} already exists.");
                        ModelState.AddModelError("Email", "Email đã được sử dụng.");
                        return View(model);
                    }

                    // Băm mật khẩu và lưu người dùng mới
                    model.HashPassword(_passwordHasher, password);
                    _context.Users.Add(model);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"User {model.UserName} registered successfully.");
                    TempData["Success"] = "Đăng ký thành công! Vui lòng đăng nhập để tiếp tục.";
                    return RedirectToAction("Login");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error occurred while registering user {model.UserName}: {ex.Message}");
                    ModelState.AddModelError("", "Đã xảy ra lỗi khi đăng ký. Vui lòng thử lại.");
                    return View(model);
                }
            }

            return View(model);
        }

        #endregion

        #region Đăng xuất (Logout)

        // GET: Account/Logout
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var userName = User.Identity.Name;
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            _logger.LogInformation($"User {userName} logged out successfully.");
            return RedirectToAction("Index", "Home");
        }

        #endregion

        #region Chỉnh sửa hồ sơ (EditProfile)

        // GET: Account/EditProfile
        [Authorize]
        public async Task<IActionResult> EditProfile()
        {
            // Lấy thông tin người dùng hiện tại
            var userName = User.Identity.Name;
            _logger.LogInformation($"Fetching profile for UserName: {userName}");

            var user = await _context.Users
                .Include(u => u.Comments)
                    .ThenInclude(c => c.Course)
                .FirstOrDefaultAsync(u => u.UserName == userName);
            if (user == null)
            {
                _logger.LogWarning($"User not found for UserName: {userName}");
                return NotFound("Không tìm thấy người dùng.");
            }

            // Lấy danh sách khóa học đã đăng ký
            var enrolledCourses = _enrollmentRepository.GetEnrolledCourses(userName);
            var enrolledCourseViewModels = enrolledCourses.Select(c => new CourseViewModel
            {
                CourseId = c.CourseId,
                Title = c.CourseName
            }).ToList();

            // Lấy danh sách bình luận
            var commentViewModels = user.Comments?.Select(c => new CommentViewModel
            {
                CommentId = c.CommentId,
                Content = c.Content,
                CommentDate = c.CreatedDate,
                CourseTitle = c.Course?.CourseName ?? "Khóa học không xác định"
            }).ToList() ?? new List<CommentViewModel>();

            // Tạo ViewModel
            var model = new UserProfileEditViewModel
            {
                UserName = user.UserName,
                FullName = user.FullName,
                Email = user.Email,
                EnrolledCourses = enrolledCourseViewModels,
                Comments = commentViewModels
            };

            return View(model);
        }

        // POST: Account/EditProfile
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(UserProfileEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Nếu ModelState không hợp lệ, cần lấy lại dữ liệu để hiển thị
                var userName = User.Identity.Name;
                var user = await _context.Users
                    .Include(u => u.Comments)
                        .ThenInclude(c => c.Course)
                    .FirstOrDefaultAsync(u => u.UserName == userName);
                if (user == null)
                {
                    _logger.LogWarning($"User not found for UserName: {userName}");
                    return NotFound("Không tìm thấy người dùng.");
                }

                var enrolledCourses = _enrollmentRepository.GetEnrolledCourses(userName);
                model.EnrolledCourses = enrolledCourses.Select(c => new CourseViewModel
                {
                    CourseId = c.CourseId,
                    Title = c.CourseName
                }).ToList();

                model.Comments = user.Comments?.Select(c => new CommentViewModel
                {
                    CommentId = c.CommentId,
                    Content = c.Content,
                    CommentDate = c.CreatedDate,
                    CourseTitle = c.Course?.CourseName ?? "Khóa học không xác định"
                }).ToList() ?? new List<CommentViewModel>();

                return View(model);
            }

            try
            {
                // Lấy thông tin người dùng hiện tại
                var userName = User.Identity.Name;
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);
                if (user == null)
                {
                    _logger.LogWarning($"User not found for UserName: {userName}");
                    return NotFound("Không tìm thấy người dùng.");
                }

                // Kiểm tra email có bị trùng không
                var existingEmail = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == model.Email && u.UserName != userName);
                if (existingEmail != null)
                {
                    _logger.LogWarning($"Email {model.Email} already exists for another user.");
                    ModelState.AddModelError("Email", "Email đã được sử dụng bởi người dùng khác.");
                    return View(model);
                }

                // Cập nhật thông tin người dùng
                user.FullName = model.FullName;
                user.Email = model.Email;

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"User {userName} updated profile successfully.");
                TempData["Success"] = "Cập nhật hồ sơ thành công!";
                return RedirectToAction("EditProfile");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred while updating profile for UserName {User.Identity.Name}: {ex.Message}");
                TempData["Error"] = "Đã có lỗi xảy ra khi cập nhật hồ sơ. Vui lòng thử lại.";
                return View(model);
            }
        }

        #endregion
    }
}