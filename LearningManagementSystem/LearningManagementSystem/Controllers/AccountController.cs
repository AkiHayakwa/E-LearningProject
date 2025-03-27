using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LearningManagementSystem.Data;
using LearningManagementSystem.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;

public class AccountController : Controller
{
    private readonly LMSContext _context;
    private readonly IPasswordHasher<User> _passwordHasher;

    public AccountController(LMSContext context, IPasswordHasher<User> passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

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

        return RedirectToAction("Index", "Home"); // Chuyển hướng đến trang chủ cho Student
    }

    // GET: Account/Register
    [AllowAnonymous]
    public IActionResult Register()
    {
        return View();
    }

    // POST: Account/Register
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(string userName, string password, string fullName, string email)
    {
        // Kiểm tra dữ liệu đầu vào
        if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(email))
        {
            ViewBag.Error = "Vui lòng nhập đầy đủ thông tin.";
            return View();
        }

        // Kiểm tra trùng lặp UserName
        if (_context.Users.Any(u => u.UserName == userName))
        {
            ViewBag.Error = "Tên đăng nhập đã tồn tại.";
            return View();
        }

        // Kiểm tra trùng lặp Email
        if (_context.Users.Any(u => u.Email == email))
        {
            ViewBag.Error = "Email đã được sử dụng.";
            return View();
        }

        // Tạo người dùng mới
        var user = new User
        {
            UserName = userName,
            FullName = fullName,
            Email = email,
            RoleId = _context.Roles.FirstOrDefault(r => r.RoleName == "Student")?.RoleId // Mặc định vai trò là Student
        };

        // Mã hóa mật khẩu
        user.Password = _passwordHasher.HashPassword(user, password);

        // Lưu vào cơ sở dữ liệu
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Thông báo thành công
        TempData["Success"] = "Đăng ký thành công! Vui lòng đăng nhập.";
        return RedirectToAction("Login");
    }

    // GET: Account/Logout
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }
}