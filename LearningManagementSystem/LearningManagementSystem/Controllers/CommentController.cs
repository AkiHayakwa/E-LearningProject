using LearningManagementSystem.Models;
using LearningManagementSystem.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LearningManagementSystem.Controllers
{
    public class CommentController : Controller
    {
        private readonly ICommentRepository _commentRepository;

        public CommentController(ICommentRepository commentRepository)
        {
            _commentRepository = commentRepository;
        }

        [HttpPost]
        public IActionResult AddComment(string courseId, string content)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            if (string.IsNullOrEmpty(content))
            {
                TempData["Error"] = "Nội dung bình luận không được để trống!";
                return RedirectToAction("CourseDetails", "Home", new { courseId });
            }

            var comment = new Comment
            {
                UserId = userId,
                CourseId = courseId,
                LessonId = null, // Bình luận trực tiếp cho khóa học
                Content = content,
                CreatedDate = DateTime.Now
            };

            _commentRepository.Add(comment);

            TempData["Success"] = "Bình luận đã được gửi thành công!";
            return RedirectToAction("CourseDetails", "Home", new { courseId });
        }
    }
}