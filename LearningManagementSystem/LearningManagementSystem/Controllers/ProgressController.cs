using LearningManagementSystem.Models;
using LearningManagementSystem.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Linq;

namespace LearningManagementSystem.Controllers
{
    [Authorize(Roles = "Student")] // Chỉ cho phép Student truy cập
    public class ProgressController : Controller
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IProgressRepository _progressRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly ILogger<ProgressController> _logger;

        public ProgressController(
            ICourseRepository courseRepository,
            IProgressRepository progressRepository,
            ILessonRepository lessonRepository,
            ILogger<ProgressController> logger)
        {
            _courseRepository = courseRepository;
            _progressRepository = progressRepository;
            _lessonRepository = lessonRepository;
            _logger = logger;
        }

        // Hiển thị tiến độ học tập của người dùng trong một khóa học
        // GET: /Progress/Index?courseId={courseId}
        public IActionResult Index(string courseId)
        {
            _logger.LogInformation($"Index called with CourseId: {courseId}");

            // Kiểm tra courseId có hợp lệ không
            if (string.IsNullOrEmpty(courseId))
            {
                _logger.LogWarning("CourseId is empty.");
                return BadRequest("CourseId không được để trống.");
            }

            // Lấy thông tin khóa học
            var course = _courseRepository.GetById(courseId);
            if (course == null)
            {
                _logger.LogWarning($"Course with CourseId: {courseId} not found.");
                return NotFound("Không tìm thấy khóa học.");
            }

            // Lấy UserName từ thông tin người dùng đã đăng nhập
            var userName = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userName))
            {
                _logger.LogError("UserName could not be determined from claims.");
                return Unauthorized("Bạn cần đăng nhập để xem tiến độ.");
            }

            // Lấy danh sách tiến độ của người dùng trong khóa học
            var progressList = _progressRepository.GetAll()
                .Where(p => p.UserName == userName && p.Lesson != null && p.Lesson.CourseId == courseId)
                .ToList();
            ViewBag.Progress = progressList;

            return View(course); // Trả về view với model là Course
        }

        // Đánh dấu một bài học là hoàn thành
        // POST: /Progress/MarkComplete
        [HttpPost]
        public IActionResult MarkComplete(string lessonId)
        {
            _logger.LogInformation($"MarkComplete called with LessonId: {lessonId}");

            // Lấy thông tin bài học
            var lesson = _lessonRepository.GetById(lessonId);
            if (lesson == null)
            {
                _logger.LogWarning($"Lesson with LessonId: {lessonId} not found.");
                return NotFound("Không tìm thấy bài học.");
            }

            // Lấy UserName từ thông tin người dùng đã đăng nhập
            var userName = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userName))
            {
                _logger.LogError("UserName could not be determined from claims.");
                return Unauthorized("Bạn cần đăng nhập để thực hiện hành động này.");
            }

            try
            {
                // Kiểm tra xem tiến độ đã tồn tại chưa
                var progress = _progressRepository.GetAll()
                    .FirstOrDefault(p => p.UserName == userName && p.LessonId == lessonId);

                if (progress == null)
                {
                    // Nếu chưa có tiến độ, tạo mới
                    progress = new Progress
                    {
                        ProgressId = Guid.NewGuid().ToString(),
                        UserName = userName,
                        LessonId = lessonId,
                        CompletionStatus = true,
                        CompletionDate = DateTime.Now
                    };
                    _progressRepository.Add(progress);
                }
                else
                {
                    // Nếu đã có tiến độ, cập nhật trạng thái
                    progress.CompletionStatus = true;
                    progress.CompletionDate = DateTime.Now;
                    _progressRepository.Update(progress);
                }

                _progressRepository.Save(); 

                _logger.LogInformation($"User {userName} marked LessonId: {lessonId} as complete.");
                return RedirectToAction("Index", new { courseId = lesson.CourseId });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred while marking lesson as complete: {ex.Message}");
                TempData["Error"] = $"Đã có lỗi xảy ra: {ex.Message}";
                return RedirectToAction("Index", new { courseId = lesson.CourseId });
            }
        }
    }
}