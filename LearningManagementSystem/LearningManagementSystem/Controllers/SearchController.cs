using Microsoft.AspNetCore.Mvc;
using LearningManagementSystem.Data;
using LearningManagementSystem.Models;
using LearningManagementSystem.Models.ViewModels;
using LearningManagementSystem.Repositories;
using System.Linq;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore; // Thêm namespace này để sử dụng .Include()

public class SearchController : Controller
{
    private readonly LMSContext _context;
    private readonly IEnrollmentRepository _enrollmentRepository;

    public SearchController(LMSContext context, IEnrollmentRepository enrollmentRepository)
    {
        _context = context;
        _enrollmentRepository = enrollmentRepository;
    }

    public IActionResult Index(string query)
    {
        // Lấy thông tin người dùng (nếu đã đăng nhập)
        var userName = User.Identity.IsAuthenticated ? User.FindFirst(ClaimTypes.NameIdentifier)?.Value : null;

        // Lấy danh sách khóa học và bao gồm Lessons
        var courses = _context.Courses
            .Include(c => c.Lessons) // Tải danh sách bài học liên quan
            .Where(c => string.IsNullOrEmpty(query) || c.CourseName.Contains(query))
            .ToList()
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
            .ToList();

        ViewBag.Query = query;
        return View(courses);
    }
}