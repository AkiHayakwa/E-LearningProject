using LearningManagementSystem.Data;
using LearningManagementSystem.Models;
using LearningManagementSystem.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LearningManagementSystem.Controllers
{
    [Authorize] // Yêu cầu người dùng đăng nhập
    public class ProgressController : Controller
    {
        private readonly ICourseRepository _courseRepository; // Repository để truy cập dữ liệu Course
        private readonly IProgressRepository _progressRepository; // Repository để truy cập dữ liệu Progress
        private readonly ILessonRepository _lessonRepository; // Repository để truy cập dữ liệu Lesson
        private readonly LMSContext _context; // DbContext để lưu thay đổi vào database

        // Constructor nhận các dependency qua Dependency Injection
        public ProgressController(
            ICourseRepository courseRepository,
            IProgressRepository progressRepository,
            ILessonRepository lessonRepository,
            LMSContext context)
        {
            _courseRepository = courseRepository;
            _progressRepository = progressRepository;
            _lessonRepository = lessonRepository;
            _context = context;
        }

        // Hiển thị tiến độ học tập của người dùng trong một khóa học
        // GET: /Progress/Index?courseId={courseId}
        public IActionResult Index(string courseId)
        {
            // Kiểm tra courseId có hợp lệ không
            if (string.IsNullOrEmpty(courseId))
            {
                return BadRequest("CourseId không được để trống.");
            }

            // Lấy thông tin khóa học, bao gồm danh sách bài học
            var course = _courseRepository.GetById(courseId);
            if (course == null)
            {
                return NotFound("Không tìm thấy khóa học.");
            }

            // Lấy UserId từ thông tin người dùng đã đăng nhập
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Bạn cần đăng nhập để xem tiến độ.");
            }

            // Lấy danh sách tiến độ của người dùng trong khóa học
            var progressList = _progressRepository.GetProgressByUserAndCourse(userId, courseId);
            ViewBag.Progress = progressList;

            return View(course); // Trả về view với model là Course
        }

        // Đánh dấu một bài học là hoàn thành
        // POST: /Progress/MarkComplete
        [HttpPost]
        public IActionResult MarkComplete(string lessonId)
        {
            // Lấy thông tin bài học
            var lesson = _lessonRepository.GetById(lessonId);
            if (lesson == null)
            {
                return NotFound("Không tìm thấy bài học.");
            }

            // Lấy UserId từ thông tin người dùng đã đăng nhập
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Bạn cần đăng nhập để thực hiện hành động này.");
            }

            // Kiểm tra xem tiến độ đã tồn tại chưa
            var progress = _progressRepository.GetProgressByUserAndCourse(userId, lesson.CourseId)
                                              .FirstOrDefault(p => p.LessonId == lessonId);

            if (progress == null)
            {
                // Nếu chưa có tiến độ, tạo mới
                progress = new Progress // Sử dụng lớp Progress (non-generic)
                {
                    ProgressId = Guid.NewGuid().ToString(), // Tạo ID duy nhất
                    UserId = userId,                        // Gán ID người dùng
                    LessonId = lessonId,                    // Gán ID bài học
                    CompletionStatus = true,                // Đánh dấu hoàn thành
                    CompletionDate = DateTime.Now           // Ghi nhận thời điểm hoàn thành
                };
                _progressRepository.Add(progress);          // Thêm vào repository
            }
            else
            {
                // Nếu đã có tiến độ, cập nhật trạng thái
                progress.CompletionStatus = true;
                progress.CompletionDate = DateTime.Now;
                _progressRepository.Update(progress);       // Cập nhật repository
            }

            _context.SaveChanges();                         // Lưu thay đổi vào database

            // Chuyển hướng về trang tiến độ của khóa học
            return RedirectToAction("Index", new { courseId = lesson.CourseId });
        }
    }
}