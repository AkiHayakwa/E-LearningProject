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

public class AccountController : Controller
{
    private readonly LMSContext _context;
    private readonly IPasswordHasher<User> _passwordHasher;

    public AccountController(LMSContext context, IPasswordHasher<User> passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
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
            return RedirectToAction("Dashboard", "Admin", new { area = "" }); // Chuyển hướng đến Dashboard
        }

        return RedirectToAction("Index", "Home"); // Chuyển hướng đến trang chủ cho Student
    }

    #endregion

    #region Đăng ký (Register)

    // GET: Account/Register
    [AllowAnonymous]
    public IActionResult Register()
    {
        ViewBag.Roles = _context.Roles.ToList();
        return View();
    }

    // POST: Account/Register
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public IActionResult Register(User model, string password, string confirmPassword)
    {
        // Kiểm tra mật khẩu và xác nhận mật khẩu
        if (password != confirmPassword)
        {
            ModelState.AddModelError("ConfirmPassword", "Mật khẩu xác nhận không khớp.");
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
                    ViewBag.Roles = _context.Roles.ToList();
                    return View(model);
                }

                // Kiểm tra email đã tồn tại chưa
                var existingEmail = _context.Users.FirstOrDefault(u => u.Email == model.Email);
                if (existingEmail != null)
                {
                    ModelState.AddModelError("Email", "Email đã được sử dụng.");
                    ViewBag.Roles = _context.Roles.ToList();
                    return View(model);
                }

                // Kiểm tra vai trò
                var role = _context.Roles.FirstOrDefault(r => r.RoleId == model.RoleId);
                if (role == null)
                {
                    ModelState.AddModelError("RoleId", "Vai trò không hợp lệ.");
                    ViewBag.Roles = _context.Roles.ToList();
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
            }
        }

        ViewBag.Roles = _context.Roles.ToList();
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
}