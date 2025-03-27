using LearningManagementSystem.Data;
using LearningManagementSystem.Models;
using LearningManagementSystem.Models.ViewModels;
using LearningManagementSystem.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace LearningManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly LMSContext _context;
        private readonly ICourseRepository _courseRepository;
        private readonly ILogger<HomeController> _logger;

        public HomeController(LMSContext context, ICourseRepository courseRepository, ILogger<HomeController> logger)
        {
            _context = context;
            _courseRepository = courseRepository;
            _logger = logger;
        }

        public IActionResult Index()
        {
            try
            {
                // Lấy thông tin người dùng (nếu đã đăng nhập)
                var userName = User.Identity.Name;
                User user = null;
                if (!string.IsNullOrEmpty(userName))
                {
                    user = _context.Users
                        .Include(u => u.Role) // Tải quan hệ Role
                        .FirstOrDefault(u => u.UserName == userName);
                    if (user != null && user.Role == null)
                    {
                        _logger.LogWarning($"User {userName} has an invalid RoleId: {user.RoleId}");
                    }
                }

                // Lấy danh sách khóa học
                var courses = _courseRepository.GetAll()?.ToList() ?? new List<Course>(); // Đảm bảo courses không bao giờ là null

                var viewModel = new HomeViewModel
                {
                    User = user,
                    Courses = courses
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Home page.");
                return View("Error");
            }
        }
    }
}