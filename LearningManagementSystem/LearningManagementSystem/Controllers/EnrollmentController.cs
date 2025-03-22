using LearningManagementSystem.Models;
using LearningManagementSystem.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LearningManagementSystem.Controllers
{
    [Authorize] // Yêu cầu người dùng đăng nhập
    public class EnrollmentController : Controller
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;

        public EnrollmentController(
            ICourseRepository courseRepository,
            IEnrollmentRepository enrollmentRepository)
        {
            _courseRepository = courseRepository;
            _enrollmentRepository = enrollmentRepository;
        }

        [HttpGet]
        public IActionResult Enroll(string courseId)
        {
            var course = _courseRepository.GetById(courseId);
            if (course == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Bạn cần đăng nhập để đăng ký khóa học.");
            }

            var existingEnrollment = _enrollmentRepository.GetEnrollmentsByUserId(userId)
                                                          .FirstOrDefault(e => e.CourseId == courseId);
            if (existingEnrollment != null)
            {
                TempData["Error"] = "Bạn đã đăng ký khóa học này.";
                return RedirectToAction("Details", "Home", new { courseId });
            }

            var enrollment = new Enrollment
            {
                EnrollmentId = Guid.NewGuid().ToString(),
                UserId = userId,
                CourseId = courseId,
                EnrollmentDate = DateTime.Now
            };

            _enrollmentRepository.Add(enrollment);

            TempData["Success"] = "Đăng ký thành công!";
            return RedirectToAction("Details", "Home", new { courseId });
        }
    }
}