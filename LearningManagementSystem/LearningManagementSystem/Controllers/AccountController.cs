using LearningManagementSystem.Data;
using LearningManagementSystem.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

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
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Tên đăng nhập và mật khẩu không được để trống.";
                return View();
            }

            var user = await _context.Users
                                     .Include(u => u.Role) // Sửa từ Roles thành Role
                                     .FirstOrDefaultAsync(u => u.UserName == username);

            if (user == null || !user.VerifyPassword(_passwordHasher, password))
            {
                ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không đúng.";
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserName),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Role, user.Role?.RoleName ?? "Guest")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            // Điều hướng dựa trên vai trò
            var role = user.Role?.RoleName;
            if (role == "Admin")
            {
                return RedirectToAction("Dashboard", "Admin");
            }
            else if (role == "Student")
            {
                return RedirectToAction("Index", "Home");
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
                bool isValid = true;

                if (string.IsNullOrWhiteSpace(username))
                {
                    ModelState.AddModelError("username", "Tên đăng nhập không được để trống.");
                    isValid = false;
                }
                else if (username.Length > 50)
                {
                    ModelState.AddModelError("username", "Tên đăng nhập không được dài quá 50 ký tự.");
                    isValid = false;
                }

                if (string.IsNullOrWhiteSpace(password))
                {
                    ModelState.AddModelError("password", "Mật khẩu không được để trống.");
                    isValid = false;
                }
                else if (password.Length > 100)
                {
                    ModelState.AddModelError("password", "Mật khẩu không được dài quá 100 ký tự.");
                    isValid = false;
                }

                if (string.IsNullOrWhiteSpace(fullName))
                {
                    ModelState.AddModelError("fullName", "Họ và tên không được để trống.");
                    isValid = false;
                }
                else if (fullName.Length > 100)
                {
                    ModelState.AddModelError("fullName", "Họ và tên không được dài quá 100 ký tự.");
                    isValid = false;
                }

                if (string.IsNullOrWhiteSpace(email))
                {
                    ModelState.AddModelError("email", "Email không được để trống.");
                    isValid = false;
                }
                else if (email.Length > 100)
                {
                    ModelState.AddModelError("email", "Email không được dài quá 100 ký tự.");
                    isValid = false;
                }
                else if (!new EmailAddressAttribute().IsValid(email))
                {
                    ModelState.AddModelError("email", "Email không hợp lệ.");
                    isValid = false;
                }

                if (isValid)
                {
                    // Kiểm tra xem username đã tồn tại chưa
                    if (_context.Users.Any(u => u.UserName == username))
                    {
                        ModelState.AddModelError("username", "Tên đăng nhập đã tồn tại.");
                        return View();
                    }

                    // Kiểm tra vai trò "role-student" có tồn tại không
                    var studentRole = _context.Roles.FirstOrDefault(r => r.RoleId == "role-student");
                    if (studentRole == null)
                    {
                        ModelState.AddModelError("", "Vai trò 'Student' không tồn tại trong hệ thống. Vui lòng liên hệ quản trị viên.");
                        return View();
                    }

                    // Tạo user mới
                    var user = new User
                    {
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
                else
                {
                    // Hiển thị lỗi validation chi tiết
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    ViewBag.Error = "Dữ liệu không hợp lệ: " + string.Join(", ", errors);
                    return View();
                }
            }
            catch (Exception ex)
            {
                // Ghi log lỗi để debug
                System.Diagnostics.Debug.WriteLine($"Lỗi khi đăng ký: {ex.Message}");
                ViewBag.Error = $"Đã có lỗi xảy ra: {ex.Message}";
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
                return RedirectToAction("Index", "Home");
            }
            return View();
        }
    }
}