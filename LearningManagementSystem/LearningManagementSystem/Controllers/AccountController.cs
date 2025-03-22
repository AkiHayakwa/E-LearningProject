using LearningManagementSystem.Data;
using LearningManagementSystem.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace LearningManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly LMSContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        public AccountController(LMSContext context, IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = await _context.Users
                                     .Include(u => u.Roles)
                                     .FirstOrDefaultAsync(u => u.UserName == username);

            if (user == null || !user.VerifyPassword(_passwordHasher, password))
            {
                ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không đúng.";
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Role, user.Roles?.RoleName ?? "Guest")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            // Điều hướng dựa trên vai trò
            var role = user.Roles?.RoleName;
            if (role == "Admin")
            {
                return RedirectToAction("Dashboard", "Admin");
            }
            else if (role == "Student")
            {
                return RedirectToAction("Dashboard", "Student");
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(string username, string password, string fullName, string email)
        {
            try
            {
                // Kiểm tra dữ liệu đầu vào
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) ||
                    string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email))
                {
                    ViewBag.Error = "Vui lòng điền đầy đủ thông tin.";
                    return View();
                }

                // Kiểm tra xem username đã tồn tại chưa
                if (_context.Users.Any(u => u.UserName == username))
                {
                    ViewBag.Error = "Tên đăng nhập đã tồn tại.";
                    return View();
                }

                // Kiểm tra email hợp lệ
                if (!email.Contains("@") || !email.Contains("."))
                {
                    ViewBag.Error = "Email không hợp lệ.";
                    return View();
                }

                // Kiểm tra vai trò "role-student" có tồn tại không
                var studentRole = _context.Roles.FirstOrDefault(r => r.RoleId == "role-student");
                if (studentRole == null)
                {
                    ViewBag.Error = "Vai trò 'Student' không tồn tại trong hệ thống. Vui lòng liên hệ quản trị viên.";
                    return View();
                }

                // Tạo user mới
                var user = new User
                {
                    UserId = Guid.NewGuid().ToString(),
                    UserName = username,
                    FullName = fullName,
                    Email = email,
                    RoleId = "role-student" // Vai trò mặc định là Student
                };

                // Băm mật khẩu trước khi lưu
                user.HashPassword(_passwordHasher, password);

                _context.Users.Add(user);
                _context.SaveChanges();

                TempData["Success"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                // Ghi log lỗi để debug
                System.Diagnostics.Debug.WriteLine($"Lỗi khi đăng ký: {ex.Message}");
                ViewBag.Error = "Đã có lỗi xảy ra. Vui lòng thử lại sau.";
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            // Điều hướng dựa trên vai trò nếu cần
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role == "Student")
            {
                return RedirectToAction("Dashboard", "Student");
            }
            return View();
        }
    }
}