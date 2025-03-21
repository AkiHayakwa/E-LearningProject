using LearningManagementSystem.Data;
using LearningManagementSystem.Models;
using LearningManagementSystem.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LearningManagementSystem.Controllers
{
    public class DiscussionController : Controller
    {
        private readonly ILessonRepository _lessonRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly LMSContext _context;

        // Constructor nhận các dependency qua DI
        public DiscussionController(ILessonRepository lessonRepository, ICommentRepository commentRepository, LMSContext context)
        {
            _lessonRepository = lessonRepository;
            _commentRepository = commentRepository;
            _context = context;
        }

        // Hiển thị danh sách bình luận cho một bài học
        public IActionResult Index(string lessonId)
        {
            var lesson = _lessonRepository.GetById(lessonId);
            if (lesson == null) return NotFound("Không tìm thấy bài học.");

            ViewBag.Comments = _commentRepository.GetCommentsByLessonId(lessonId);
            return View(lesson);
        }

        // Thêm bình luận mới (yêu cầu đăng nhập)
        [Authorize]
        [HttpPost]
        public IActionResult AddComment(string lessonId, string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                ModelState.AddModelError("", "Nội dung bình luận không được để trống.");
                return RedirectToAction("Index", new { lessonId });
            }

            var comment = new Comment
            {
                CommentId = Guid.NewGuid().ToString(),
                LessonId = lessonId,
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier), // Lấy UserId từ Identity
                Content = content,
                CreatedDate = DateTime.Now
            };

            _commentRepository.Add(comment);
            _context.SaveChanges(); // Lưu thay đổi vào database

            return RedirectToAction("Index", new { lessonId });
        }

        // Xóa bình luận (chỉ Admin hoặc Instructor)
        [Authorize(Roles = "Admin,Instructor")]
        [HttpPost]
        public IActionResult DeleteComment(string id, string lessonId)
        {
            var comment = _commentRepository.GetById(id);
            if (comment == null) return NotFound("Không tìm thấy bình luận.");

            _commentRepository.Delete(comment);
            _context.SaveChanges(); // Lưu thay đổi vào database

            return RedirectToAction("Index", new { lessonId });
        }
    }
}
