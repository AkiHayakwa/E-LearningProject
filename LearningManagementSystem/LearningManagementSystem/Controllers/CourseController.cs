using LearningManagementSystem.Data;
using LearningManagementSystem.Models;
using LearningManagementSystem.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LearningManagementSystem.Controllers
{
    public class CourseController : Controller
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly LMSContext _context;

        public CourseController(ICourseRepository courseRepository, IEnrollmentRepository enrollmentRepository, LMSContext context)
        {
            _courseRepository = courseRepository;
            _enrollmentRepository = enrollmentRepository;
            _context = context;
        }

        public IActionResult Index()
        {
            var courses = _courseRepository.GetAll();
            return View(courses);
        }

        public IActionResult Details(string courseId)
        {
            var course = _courseRepository.GetById(courseId);
            if (course == null) return NotFound();
            return View(course);
        }

        [Authorize]
        [HttpPost]
        public IActionResult Enroll(string courseId)
        {
            var course = _courseRepository.GetById(courseId);
            if (course == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var existingEnrollment = _enrollmentRepository.GetEnrollmentsByUserId(userId)
                                                          .FirstOrDefault(e => e.CourseId == courseId);
            if (existingEnrollment != null)
            {
                TempData["Error"] = "Bạn đã đăng ký khóa học này.";
                return RedirectToAction("Details", new { courseId });
            }

            var enrollment = new Enrollment
            {
                EnrollmentId = Guid.NewGuid().ToString(),
                UserId = userId,
                CourseId = courseId,
                EnrollmentDate = DateTime.Now
            };

            _enrollmentRepository.Add(enrollment);
            _context.SaveChanges();

            TempData["Success"] = "Đăng ký thành công!";
            return RedirectToAction("Details", new { courseId });
        }
    }
}
