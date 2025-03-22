﻿using LearningManagementSystem.Data;
using LearningManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LearningManagementSystem.Controllers
{
    public class EnrollmentController : Controller
    {
        private readonly LMSContext _context;

        public EnrollmentController(LMSContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Enroll(string courseId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            // Kiểm tra xem người dùng đã đăng ký khóa học này chưa
            var existingEnrollment = _context.Enrollments
                                            .FirstOrDefault(e => e.UserId == userId && e.CourseId == courseId);
            if (existingEnrollment != null)
            {
                TempData["Error"] = "Bạn đã đăng ký khóa học này rồi!";
                return RedirectToAction("CourseDetails", "Home", new { courseId });
            }

            // Tạo bản ghi đăng ký mới
            var enrollment = new Enrollment
            {
                UserId = userId,
                CourseId = courseId,
                EnrollmentDate = DateTime.Now
            };

            _context.Enrollments.Add(enrollment);
            _context.SaveChanges();

            TempData["Success"] = "Đăng ký khóa học thành công!";
            return RedirectToAction("CourseDetails", "Home", new { courseId });
        }
    }
}