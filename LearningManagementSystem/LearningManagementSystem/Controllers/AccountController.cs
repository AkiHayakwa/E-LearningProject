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
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Vui lòng nhập tên đăng nhập và mật khẩu.";
                return View();
            }

            // Tải quan hệ Role cùng với User
            var user = _context.Users
                .Include(u => u.Role)
                .FirstOrDefault(u => u.UserName == userName);

            if (user == null)
            {
                ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không đúng.";
                return View();
            }

            // Kiểm tra mật khẩu
            var result = _passwordHasher.VerifyHashedPassword(user, user.Password, password);
            if (result != PasswordVerificationResult.Success)
            {
                ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không đúng.";
                return View();
            }

            // Kiểm tra user.Role có null không
            if (user.Role == null)
            {
                ViewBag.Error = "Không tìm thấy vai trò của người dùng. Vui lòng liên hệ quản trị viên.";
                return View();
            }

            // Tạo claims cho người dùng
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.UserName),
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
        public IActionResult Register(User model, string password, string confirmPassword)
        {
            // Kiểm tra mật khẩu và xác nhận mật khẩu
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword) || password != confirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "Mật khẩu xác nhận không khớp.");
            }

            // Tự động gán vai trò "Student" trước khi kiểm tra ModelState
            var studentRole = _context.Roles.FirstOrDefault(r => r.RoleName == "Student");
            if (studentRole == null)
            {
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
                    var existingUser = _context.Users.FirstOrDefault(u => u.UserName == model.UserName);
                    if (existingUser != null)
                    {
                        ModelState.AddModelError("UserName", "Tên đăng nhập đã tồn tại.");
                        return View(model);
                    }

                    // Kiểm tra email đã tồn tại chưa
                    var existingEmail = _context.Users.FirstOrDefault(u => u.Email == model.Email);
                    if (existingEmail != null)
                    {
                        ModelState.AddModelError("Email", "Email đã được sử dụng.");
                        return View(model);
                    }

                    // Băm mật khẩu và lưu người dùng mới
                    model.HashPassword(_passwordHasher, password);
                    _context.Users.Add(model);
                    _context.SaveChanges();

                    TempData["Success"] = "Đăng ký thành công! Vui lòng đăng nhập để tiếp tục.";
                    return RedirectToAction("Login");
                }
                catch (Exception ex)
                {
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
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        #endregion

        #region Chỉnh sửa hồ sơ (EditProfile)

        // GET: Account/EditProfile
        [Authorize]
        public IActionResult EditProfile()
        {
            // Lấy thông tin người dùng hiện tại
            var userName = User.Identity.Name;
            var user = _context.Users
                .Include(u => u.Comments)
                    .ThenInclude(c => c.Course)
                .FirstOrDefault(u => u.UserName == userName);
            if (user == null)
            {
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
                CommentId = c.CommentId, // Đã sửa thành string
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
        public IActionResult EditProfile(UserProfileEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Nếu ModelState không hợp lệ, cần lấy lại dữ liệu để hiển thị
                var userName = User.Identity.Name;
                var user = _context.Users
                    .Include(u => u.Comments)
                        .ThenInclude(c => c.Course)
                    .FirstOrDefault(u => u.UserName == userName);
                if (user == null)
                {
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
                    CommentId = c.CommentId, // Đã sửa thành string
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
                var user = _context.Users.FirstOrDefault(u => u.UserName == userName);
                if (user == null)
                {
                    return NotFound("Không tìm thấy người dùng.");
                }

                // Cập nhật thông tin người dùng
                user.FullName = model.FullName;
                user.Email = model.Email;

                _context.Users.Update(user);
                _context.SaveChanges();

                TempData["Success"] = "Cập nhật hồ sơ thành công!";
                return RedirectToAction("EditProfile");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred while updating profile: {ex.Message}");
                TempData["Error"] = "Đã có lỗi xảy ra khi cập nhật hồ sơ. Vui lòng thử lại.";
                return View(model);
            }
        }

        #endregion
    }
}