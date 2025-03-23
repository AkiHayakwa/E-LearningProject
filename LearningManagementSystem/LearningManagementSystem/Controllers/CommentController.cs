using LearningManagementSystem.Models;
using LearningManagementSystem.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

public class CommentController : Controller
{
    private readonly ICommentRepository _commentRepository;
    private readonly ILessonRepository _lessonRepository;

    public CommentController(ICommentRepository commentRepository, ILessonRepository lessonRepository)
    {
        _commentRepository = commentRepository;
        _lessonRepository = lessonRepository;
    }

    [HttpGet]
    public IActionResult GetCommentsByCourse(string courseId)
    {
        if (string.IsNullOrWhiteSpace(courseId))
        {
            return BadRequest("Course ID is required.");
        }

        var comments = _commentRepository.GetCommentsByCourse(courseId);
        var commentList = comments.Select(c => new
        {
            userName = c.User?.FullName ?? "Unknown",
            content = c.Content,
            createdDate = c.CreatedDate.ToString("dd/MM/yyyy HH:mm"),
            lessonTitle = c.Lesson?.LessonTitle ?? "N/A"
        });

        return Json(commentList);
    }

    [Authorize]
    [HttpPost]
    public IActionResult AddComment(string courseId, string lessonId, string content)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(courseId) || string.IsNullOrWhiteSpace(lessonId) || string.IsNullOrWhiteSpace(content))
            {
                return Json(new { success = false, message = "Thông tin không hợp lệ." });
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Json(new { success = false, message = "Không thể xác định người dùng. Vui lòng đăng nhập lại." });
            }

            var lesson = _lessonRepository.GetById(lessonId);
            if (lesson == null || lesson.CourseId != courseId)
            {
                return Json(new { success = false, message = "Bài học không tồn tại hoặc không thuộc khóa học này." });
            }

            var comment = new Comment
            {
                CommentId = Guid.NewGuid().ToString(),
                UserId = userId,
                LessonId = lessonId,
                Content = content,
                CreatedDate = DateTime.Now
            };

            _commentRepository.Add(comment);
            _commentRepository.Save();

            return Json(new { success = true, message = "Bình luận đã được thêm thành công!" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"Đã có lỗi xảy ra: {ex.Message}" });
        }
    }

    // Lấy bài học đầu tiên của khóa học (dùng cho modal bình luận)
    [HttpGet]
    public IActionResult GetFirstLesson(string courseId)
    {
        if (string.IsNullOrWhiteSpace(courseId))
        {
            return BadRequest("Course ID is required.");
        }

        var lesson = _lessonRepository.GetLessonsByCourse(courseId)
            .OrderBy(l => l.LessonId)
            .FirstOrDefault();

        if (lesson == null)
        {
            return Json(new { success = false, message = "Khóa học này chưa có bài học." });
        }

        return Json(new { success = true, lessonId = lesson.LessonId });
    }
}