using LearningManagementSystem.Models;
using LearningManagementSystem.Models.ViewModels;
using LearningManagementSystem.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace LearningManagementSystem.Controllers
{
    [Authorize]
    public class LessonController : Controller
    {
        private readonly ILessonRepository _lessonRepository;
        private readonly IProgressRepository _progressRepository;

        public LessonController(ILessonRepository lessonRepository, IProgressRepository progressRepository)
        {
            _lessonRepository = lessonRepository;
            _progressRepository = progressRepository;
        }

        [HttpGet]
        public IActionResult ViewLesson(string lessonId)
        {
            // Lấy thông tin bài học
            var lesson = _lessonRepository.GetById(lessonId);
            if (lesson == null)
            {
                return NotFound();
            }

            // Lấy userId từ thông tin người dùng hiện tại (dựa trên Claims)
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // Lấy tiến độ của người dùng cho bài học này
            var progress = _progressRepository.GetProgressByUserAndCourse(userId, lesson.CourseId)
                                             .FirstOrDefault(p => p.LessonId == lessonId);

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