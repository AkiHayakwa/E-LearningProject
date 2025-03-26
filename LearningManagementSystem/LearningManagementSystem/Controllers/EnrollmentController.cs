using LearningManagementSystem.Models;
using LearningManagementSystem.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

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

            // Kiểm tra xem người dùng đã đăng ký khóa học chưa
            var existingEnrollment = _enrollmentRepository.GetAll()
                .FirstOrDefault(e => e.UserName == userName && e.CourseId == courseId);
            if (existingEnrollment != null)
            {
                _logger.LogWarning($"User {userName} has already enrolled in CourseId: {courseId}.");
                TempData["Error"] = "Bạn đã đăng ký khóa học này rồi.";
                return RedirectToAction("Index", "Home");
            }

            // Tạo bản ghi đăng ký mới
            var enrollment = new Enrollment
            {
                EnrollmentId = Guid.NewGuid().ToString(),
                UserName = userName,
                CourseId = courseId,
                EnrollmentDate = DateTime.Now
            };

            _enrollmentRepository.Add(enrollment);
            _enrollmentRepository.Save();

            _logger.LogInformation($"User {userName} successfully enrolled in CourseId: {courseId}.");
            TempData["Success"] = "Đăng ký khóa học thành công!";
            return RedirectToAction("Index", "Home");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error occurred while enrolling: {ex.Message}");
            TempData["Error"] = $"Đã có lỗi xảy ra: {ex.Message}";
            return RedirectToAction("Index", "Home");
        }
    }
}