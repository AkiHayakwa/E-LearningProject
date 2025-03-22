using LearningManagementSystem.Data;
using LearningManagementSystem.Models;
using LearningManagementSystem.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LearningManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly LMSContext _context;

        public HomeController(LMSContext context)
        {
            _context = context;
        }

        // Trang dashboard
        public IActionResult Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = _context.Users.Include(u => u.Roles)
                                     .FirstOrDefault(u => u.UserId == userId);
            var enrollments = _context.Enrollments.Include(e => e.Course)
                                                  .Where(e => e.UserId == userId)
                                                  .ToList();
            var progresses = _context.Progresses.Include(p => p.Lesson)
                                                .ThenInclude(l => l.Course)
                                                .Where(p => p.UserId == userId)
                                                .OrderByDescending(p => p.CompletionDate ?? DateTime.MinValue)
                                                .Take(5)
                                                .ToList();
            var comments = _context.Comments.Include(c => c.User)
                                            .Include(c => c.Lesson)
                                            .ThenInclude(l => l.Course)
                                            .Where(c => c.LessonId != "notification")
                                            .OrderByDescending(c => c.CreatedDate)
                                            .Take(5)
                                            .ToList();
            var notifications = _context.Comments
                                        .Where(c => c.LessonId == "notification" && c.UserId == userId)
                                        .OrderByDescending(c => c.CreatedDate)
                                        .Take(5)
                                        .ToList();

            var viewModel = new HomeViewModel
            {
                User = user,
                Enrollments = enrollments,
                Progresses = progresses,
                Comments = comments,
                Notifications = notifications
            };

            return View(viewModel);
        }

        // Trang danh sách khóa học
        public IActionResult Courses()
        {
            var courses = _context.Courses.Include(c => c.Instructor)
                                          .ToList();
            return View(courses);
        }

        // Trang chi tiết khóa học
        public IActionResult CourseDetails(string courseId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var course = _context.Courses.Include(c => c.Instructor)
                                         .FirstOrDefault(c => c.CourseId == courseId);
            if (course == null)
            {
                return NotFound();
            }

            var isEnrolled = _context.Enrollments
                                     .Any(e => e.UserId == userId && e.CourseId == courseId);

            var comments = _context.Comments
                                  .Include(c => c.User)
                                  .Where(c => c.CourseId == courseId && c.LessonId == null)
                                  .OrderByDescending(c => c.CreatedDate)
                                  .ToList();

            var viewModel = new CourseDetailsViewModel
            {
                Course = course,
                IsEnrolled = isEnrolled,
                Comments = comments
            };

            return View(viewModel);
        }
    }
}