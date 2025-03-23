using LearningManagementSystem.Models;
using LearningManagementSystem.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using Microsoft.Extensions.Logging;

[Authorize]
public class CommentController : Controller
{
    private readonly ICommentRepository _commentRepository;
    private readonly ILessonRepository _lessonRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly ILogger<CommentController> _logger;

    public CommentController(
        ICommentRepository commentRepository,
        ILessonRepository lessonRepository,
        ICourseRepository courseRepository,
        IEnrollmentRepository enrollmentRepository,
        ILogger<CommentController> logger)
    {
        _commentRepository = commentRepository;
        _lessonRepository = lessonRepository;
        _courseRepository = courseRepository;
        _enrollmentRepository = enrollmentRepository;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult GetCommentsByCourse(string courseId)
    {
        _logger.LogInformation($"GetCommentsByCourse called with CourseId: {courseId}");

        if (string.IsNullOrEmpty(courseId))
        {
            return Json(new { success = false, message = "CourseId không hợp lệ." });
        }

        var comments = _commentRepository.GetAll()
            .Where(c => c.CourseId == courseId)
            .Select(c => new
            {
                userName = c.User?.FullName ?? "Người dùng không xác định",
                lessonTitle = c.Lesson?.LessonTitle, // Có thể null nếu không gắn với bài học
                content = c.Content,
                createdDate = c.CreatedDate.ToString("dd/MM/yyyy HH:mm")
            })
            .ToList();

        return Json(comments);
    }

    [HttpPost]
    public IActionResult AddComment(string content, string courseId, string lessonId = null)
    {
        _logger.LogInformation($"AddComment called with Content: {content}, CourseId: {courseId}, LessonId: {lessonId}");

        // Kiểm tra các trường cần thiết
        if (string.IsNullOrEmpty(content) || content.Length > 1000)
        {
            return Json(new { success = false, message = "Nội dung bình luận không hợp lệ." });
        }

        if (string.IsNullOrEmpty(courseId))
        {
            return Json(new { success = false, message = "CourseId không hợp lệ." });
        }

        try
        {
            // Kiểm tra khóa học
            var course = _courseRepository.GetById(courseId);
            if (course == null)
            {
                _logger.LogError($"Course with CourseId: {courseId} not found.");
                return Json(new { success = false, message = "Khóa học không tồn tại." });
            }

            // Kiểm tra bài học (nếu có)
            Lesson lesson = null;
            if (!string.IsNullOrEmpty(lessonId))
            {
                lesson = _lessonRepository.GetById(lessonId);
                if (lesson == null)
                {
                    _logger.LogError($"Lesson with LessonId: {lessonId} not found.");
                    return Json(new { success = false, message = "Bài học không tồn tại." });
                }
            }

            // Lấy UserId từ thông tin đăng nhập
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogError("UserId could not be determined from claims.");
                return Json(new { success = false, message = "Không thể xác định người dùng." });
            }

            // Kiểm tra xem người dùng đã đăng ký khóa học chưa
            var enrollment = _enrollmentRepository.GetAll()
                .FirstOrDefault(e => e.UserId == userId && e.CourseId == courseId);
            if (enrollment == null)
            {
                _logger.LogError($"Enrollment not found for UserId: {userId}, CourseId: {courseId}");
                return Json(new { success = false, message = "Bạn chưa đăng ký khóa học này." });
            }

            // Tạo bình luận mới
            var comment = new Comment
            {
                CommentId = Guid.NewGuid().ToString(),
                Content = content,
                CreatedDate = DateTime.Now,
                UserId = userId,
                LessonId = lessonId, // Có thể null
                CourseId = courseId
            };

            _commentRepository.Add(comment);
            _commentRepository.Save();

            return Json(new { success = true, message = "Thêm bình luận thành công!" });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error occurred while adding comment: {ex.Message}");
            return Json(new { success = false, message = $"Đã có lỗi xảy ra: {ex.Message}" });
        }
    }
}