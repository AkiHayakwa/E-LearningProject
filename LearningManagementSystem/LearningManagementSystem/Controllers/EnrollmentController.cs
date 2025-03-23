using LearningManagementSystem.Models;
using LearningManagementSystem.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

public class EnrollmentController : Controller
{
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly ICourseRepository _courseRepository;

    public EnrollmentController(
        IEnrollmentRepository enrollmentRepository,
        ICourseRepository courseRepository)
    {
        _enrollmentRepository = enrollmentRepository;
        _courseRepository = courseRepository;
    }

    [Authorize]
    [HttpPost]
    public IActionResult Enroll(string id)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                TempData["Error"] = "ID khóa học không hợp lệ.";
                return RedirectToAction("Index", "Home");
            }

            var course = _courseRepository.GetById(id);
            if (course == null)
            {
                TempData["Error"] = "Khóa học không tồn tại.";
                return RedirectToAction("Index", "Home");
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userId))
            {
                TempData["Error"] = "Không thể xác định người dùng. Vui lòng đăng nhập lại.";
                return RedirectToAction("Login", "Account");
            }

            var existingEnrollment = _enrollmentRepository.GetEnrollment(userId, id);
            if (existingEnrollment != null)
            {
                TempData["Error"] = "Bạn đã đăng ký khóa học này rồi.";
                return RedirectToAction("Index", "Home");
            }

            var enrollment = new Enrollment
            {
                EnrollmentId = Guid.NewGuid().ToString(),
                UserId = userId,
                CourseId = id,
                EnrollmentDate = DateTime.Now
            };

            _enrollmentRepository.Add(enrollment);
            _enrollmentRepository.Save();

            TempData["Success"] = "Đăng ký khóa học thành công!";
            return RedirectToAction("Index", "Home");
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Đã có lỗi xảy ra: {ex.Message}";
            return RedirectToAction("Index", "Home");
        }
    }
}