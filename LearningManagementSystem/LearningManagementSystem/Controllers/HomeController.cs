using LearningManagementSystem.Data;
using LearningManagementSystem.Models;
using LearningManagementSystem.Models.ViewModels;
using LearningManagementSystem.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;

namespace LearningManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly LMSContext _context;
        private readonly ICourseRepository _courseRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            LMSContext context,
            ICourseRepository courseRepository,
            IEnrollmentRepository enrollmentRepository,
            ILogger<HomeController> logger)
        {
            _context = context;
            _courseRepository = courseRepository;
            _enrollmentRepository = enrollmentRepository;
            _logger = logger;
        }

        public IActionResult Index()
        {
            try
            {
                // Lấy thông tin người dùng (nếu đã đăng nhập)
                var userName = User.Identity.IsAuthenticated ? User.FindFirst(ClaimTypes.NameIdentifier)?.Value : null;
                User user = null;
                if (!string.IsNullOrEmpty(userName))
                {
                    user = _context.Users
                        .Include(u => u.Role)
                        .FirstOrDefault(u => u.UserName == userName);
                    if (user != null && user.Role == null)
                    {
                        _logger.LogWarning($"User {userName} has an invalid RoleId: {user.RoleId}");
                    }
                }

                // Lấy danh sách khóa học và bao gồm Lessons
                var courses = _courseRepository.GetAll()
                    .Include(c => c.Lessons) // Tải danh sách bài học liên quan
                    .Select(c => new CourseViewModel
                    {
                        CourseId = c.CourseId,
                        CourseName = c.CourseName,
                        Title = c.CourseName,
                        Description = c.Description,
                        CreatedDate = c.CreatedDate,
                        ImageUrl = c.ImageUrl,
                        Lessons = c.Lessons,
                        IsEnrolled = userName != null && _enrollmentRepository.GetAll()
                            .Any(e => e.UserName == userName && e.CourseId == c.CourseId)
                    })
                    .ToList() ?? new List<CourseViewModel>();

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