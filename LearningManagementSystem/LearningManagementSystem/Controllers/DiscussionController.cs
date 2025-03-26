using LearningManagementSystem.Data;
using LearningManagementSystem.Models;
using LearningManagementSystem.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace LearningManagementSystem.Controllers
{
    [Authorize]
    public class DiscussionController : Controller
    {
        private readonly ILessonRepository _lessonRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly ILogger<DiscussionController> _logger;

        public DiscussionController(
            ILessonRepository lessonRepository,
            ICommentRepository commentRepository,
            ICourseRepository courseRepository,
            IEnrollmentRepository enrollmentRepository,
            ILogger<DiscussionController> logger)
        {
            _lessonRepository = lessonRepository;
            _commentRepository = commentRepository;
            _courseRepository = courseRepository;
            _enrollmentRepository = enrollmentRepository;
            _logger = logger;
        }

        // Hiển thị danh sách bình luận cho một bài học (dựa trên CourseId của bài học)
        public IActionResult Index(string lessonId)
        {
            _logger.LogInformation($"Index called with LessonId: {lessonId}");

            var lesson = _lessonRepository.GetById(lessonId);
            if (lesson == null)
            {
                _logger.LogWarning($"Lesson with LessonId: {lessonId} not found.");
                return NotFound("Không tìm thấy bài học.");
            }

            var course = _courseRepository.GetById(lesson.CourseId);
            if (course == null)
            {
                _logger.LogWarning($"Course with CourseId: {lesson.CourseId} not found.");
                return NotFound("Không tìm thấy khóa học.");
            }

            // Lấy danh sách bình luận theo CourseId
            var comments = _commentRepository.GetAll()
                .Where(c => c.CourseId == lesson.CourseId)
                .Select(c => new
                {
                    userName = c.User != null && c.User.FullName != null ? c.User.FullName : "Người dùng không xác định",
                    content = c.Content,
                    createdDate = c.CreatedDate.ToString("dd/MM/yyyy HH:mm")
                })
                .ToList();

            ViewBag.Comments = comments;
            ViewBag.CourseId = lesson.CourseId; // Truyền CourseId để sử dụng trong form thêm bình luận
            return View(lesson);
        }

        // Thêm bình luận mới (yêu cầu đăng nhập và đã đăng ký khóa học)
        [HttpPost]
        public IActionResult AddComment(string courseId, string content)
        {
            _logger.LogInformation($"AddComment called with CourseId: {courseId}, Content: {content}");

            // Kiểm tra nội dung bình luận
            if (string.IsNullOrEmpty(content) || content.Length > 1000)
            {
                _logger.LogWarning("Invalid comment content.");
                TempData["Error"] = "Nội dung bình luận không hợp lệ (tối đa 1000 ký tự).";
                return RedirectToAction("Index", "Course", new { courseId });
            }

            try
            {
                // Kiểm tra khóa học
                var course = _courseRepository.GetById(courseId);
                if (course == null)
                {
                    _logger.LogWarning($"Course with CourseId: {courseId} not found.");
                    return NotFound("Không tìm thấy khóa học.");
                }

                // Lấy UserName từ thông tin đăng nhập
                var userName = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userName))
                {
                    _logger.LogError("UserName could not be determined from claims.");
                    TempData["Error"] = "Không thể xác định người dùng.";
                    return RedirectToAction("Index", "Course", new { courseId });
                }

                // Kiểm tra xem người dùng đã đăng ký khóa học chưa
                var enrollment = _enrollmentRepository.GetAll()
                    .FirstOrDefault(e => e.UserName == userName && e.CourseId == courseId);
                if (enrollment == null)
                {
                    _logger.LogWarning($"User {userName} has not enrolled in CourseId: {courseId}.");
                    TempData["Error"] = "Bạn chưa đăng ký khóa học này.";
                    return RedirectToAction("Index", "Course", new { courseId });
                }

                // Tạo bình luận mới
                var comment = new Comment
                {
                    CommentId = Guid.NewGuid().ToString(),
                    CourseId = courseId,
                    UserName = userName,
                    Content = content,
                    CreatedDate = DateTime.Now
                };

                _commentRepository.Add(comment);
                _commentRepository.Save();

                TempData["Success"] = "Thêm bình luận thành công!";
                return RedirectToAction("Index", "Course", new { courseId });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred while adding comment: {ex.Message}");
                TempData["Error"] = $"Đã có lỗi xảy ra: {ex.Message}";
                return RedirectToAction("Index", "Course", new { courseId });
            }
        }

        // Xóa bình luận (chỉ Admin)
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult DeleteComment(string id, string courseId)
        {
            _logger.LogInformation($"DeleteComment called with CommentId: {id}, CourseId: {courseId}");

            var comment = _commentRepository.GetById(id);
            if (comment == null)
            {
                _logger.LogWarning($"Comment with CommentId: {id} not found.");
                return NotFound("Không tìm thấy bình luận.");
            }

            try
            {
                _commentRepository.Delete(id);
                _commentRepository.Save();

                TempData["Success"] = "Xóa bình luận thành công!";
                return RedirectToAction("Index", "Course", new { courseId });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred while deleting comment: {ex.Message}");
                TempData["Error"] = $"Đã có lỗi xảy ra: {ex.Message}";
                return RedirectToAction("Index", "Course", new { courseId });
            }
        }
    }
}