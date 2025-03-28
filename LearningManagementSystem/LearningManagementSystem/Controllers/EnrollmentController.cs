using LearningManagementSystem.Models;
using LearningManagementSystem.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims;

namespace LearningManagementSystem.Controllers
{
    [Authorize(Roles = "Student")]
    public class EnrollmentController : Controller
    {
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly ILogger<EnrollmentController> _logger;

        public EnrollmentController(
            IEnrollmentRepository enrollmentRepository,
            ICourseRepository courseRepository,
            ILogger<EnrollmentController> logger)
        {
            _enrollmentRepository = enrollmentRepository;
            _courseRepository = courseRepository;
            _logger = logger;
        }

        // POST: Enrollment/Enroll
        [HttpPost]
        public IActionResult Enroll(string courseId)
        {
            _logger.LogInformation($"Enroll called with CourseId: {courseId}");

            try
            {
                // Kiểm tra CourseId
                if (string.IsNullOrWhiteSpace(courseId))
                {
                    _logger.LogWarning("Invalid CourseId.");
                    TempData["Error"] = "ID khóa học không hợp lệ.";
                    return RedirectToAction("Index", "Home");
                }

                // Kiểm tra khóa học
                var course = _courseRepository.GetById(courseId);
                if (course == null)
                {
                    _logger.LogWarning($"Course with CourseId: {courseId} not found.");
                    TempData["Error"] = "Khóa học không tồn tại.";
                    return RedirectToAction("Index", "Home");
                }

                // Lấy UserName từ thông tin đăng nhập
                var userName = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrWhiteSpace(userName))
                {
                    _logger.LogError("UserName could not be determined from claims.");
                    TempData["Error"] = "Không thể xác định người dùng. Vui lòng đăng nhập lại.";
                    return RedirectToAction("Login", "Account");
                }

                // Sử dụng phương thức Enroll từ IEnrollmentRepository
                var success = _enrollmentRepository.Enroll(userName, courseId);
                if (success)
                {
                    _logger.LogInformation($"User {userName} successfully enrolled in CourseId: {courseId}.");
                    TempData["Success"] = "Đăng ký khóa học thành công!";
                }
                else
                {
                    _logger.LogWarning($"User {userName} has already enrolled in CourseId: {courseId} or enrollment failed.");
                    TempData["Error"] = "Bạn đã đăng ký khóa học này rồi hoặc đăng ký thất bại.";
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred while enrolling: {ex.Message}");
                TempData["Error"] = $"Đã có lỗi xảy ra: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        // POST: Enrollment/Unenroll
        [HttpPost]
        public IActionResult Unenroll(string courseId)
        {
            _logger.LogInformation($"Unenroll called with CourseId: {courseId}");

            try
            {
                // Kiểm tra CourseId
                if (string.IsNullOrWhiteSpace(courseId))
                {
                    _logger.LogWarning("Invalid CourseId.");
                    TempData["Error"] = "ID khóa học không hợp lệ.";
                    return RedirectToAction("Index", "Home");
                }

                // Kiểm tra khóa học
                var course = _courseRepository.GetById(courseId);
                if (course == null)
                {
                    _logger.LogWarning($"Course with CourseId: {courseId} not found.");
                    TempData["Error"] = "Khóa học không tồn tại.";
                    return RedirectToAction("Index", "Home");
                }

                // Lấy UserName từ thông tin đăng nhập
                var userName = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrWhiteSpace(userName))
                {
                    _logger.LogError("UserName could not be determined from claims.");
                    TempData["Error"] = "Không thể xác định người dùng. Vui lòng đăng nhập lại.";
                    return RedirectToAction("Login", "Account");
                }

                // Sử dụng phương thức Unenroll từ IEnrollmentRepository
                var success = _enrollmentRepository.Unenroll(userName, courseId);
                if (success)
                {
                    _logger.LogInformation($"User {userName} successfully unenrolled from CourseId: {courseId}.");
                    TempData["Success"] = "Hủy đăng ký khóa học thành công!";
                }
                else
                {
                    _logger.LogWarning($"User {userName} has not enrolled in CourseId: {courseId} or unenrollment failed.");
                    TempData["Error"] = "Bạn chưa đăng ký khóa học này hoặc hủy đăng ký thất bại.";
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred while unenrolling: {ex.Message}");
                TempData["Error"] = $"Đã có lỗi xảy ra: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }
    }
}