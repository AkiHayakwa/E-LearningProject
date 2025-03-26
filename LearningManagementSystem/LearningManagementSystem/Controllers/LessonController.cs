using LearningManagementSystem.Models;
using LearningManagementSystem.Models.ViewModels;
using LearningManagementSystem.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace LearningManagementSystem.Controllers
{
    [Authorize(Roles = "Student")] // Chỉ cho phép Student truy cập
    public class LessonController : Controller
    {
        private readonly ILessonRepository _lessonRepository;
        private readonly IProgressRepository _progressRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly ILogger<LessonController> _logger;

        public LessonController(
            ILessonRepository lessonRepository,
            IProgressRepository progressRepository,
            IEnrollmentRepository enrollmentRepository,
            ICourseRepository courseRepository,
            ILogger<LessonController> logger)
        {
            _lessonRepository = lessonRepository;
            _progressRepository = progressRepository;
            _enrollmentRepository = enrollmentRepository;
            _courseRepository = courseRepository;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult ViewLesson(string lessonId)
        {
            _logger.LogInformation($"ViewLesson called with LessonId: {lessonId}");

            // Kiểm tra lessonId có hợp lệ không
            if (string.IsNullOrEmpty(lessonId))
            {
                _logger.LogWarning("LessonId is empty.");
                return BadRequest("LessonId không được để trống.");
            }

            // Lấy thông tin bài học
            var lesson = _lessonRepository.GetById(lessonId);
            if (lesson == null)
            {
                _logger.LogWarning($"Lesson with LessonId: {lessonId} not found.");
                return NotFound("Không tìm thấy bài học.");
            }

            // Lấy thông tin khóa học
            var course = _courseRepository.GetById(lesson.CourseId);
            if (course == null)
            {
                _logger.LogWarning($"Course with CourseId: {lesson.CourseId} not found.");
                return NotFound("Không tìm thấy khóa học.");
            }

            // Lấy UserName từ thông tin người dùng hiện tại
            var userName = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userName))
            {
                _logger.LogError("UserName could not be determined from claims.");
                return Unauthorized("Bạn cần đăng nhập để xem bài học.");
            }

            // Kiểm tra xem người dùng đã đăng ký khóa học chưa
            var enrollment = _enrollmentRepository.GetAll()
                .FirstOrDefault(e => e.UserName == userName && e.CourseId == lesson.CourseId);
            if (enrollment == null)
            {
                _logger.LogWarning($"User {userName} has not enrolled in CourseId: {lesson.CourseId}.");
                TempData["Error"] = "Bạn chưa đăng ký khóa học này.";
                return RedirectToAction("Index", "Home");
            }

            // Lấy tiến độ của người dùng cho bài học này
            var progress = _progressRepository.GetAll()
                .FirstOrDefault(p => p.UserName == userName && p.LessonId == lessonId);

            // Tạo ViewModel để hiển thị thông tin bài học và tiến độ
            var viewModel = new LessonViewModel
            {
                Lesson = lesson,
                Progress = progress
            };

            return View(viewModel);
        }
    }
}