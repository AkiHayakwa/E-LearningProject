using LearningManagementSystem.Models;
using LearningManagementSystem.Models.ViewModels;
using LearningManagementSystem.Repositories;
using LearningManagementSystem.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace LearningManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly IProgressRepository _progressRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;

        public AdminController(
            IUserRepository userRepository,
            ICourseRepository courseRepository,
            ILessonRepository lessonRepository,
            ICommentRepository commentRepository,
            IProgressRepository progressRepository,
            IEnrollmentRepository enrollmentRepository)
        {
            _userRepository = userRepository;
            _courseRepository = courseRepository;
            _lessonRepository = lessonRepository;
            _commentRepository = commentRepository;
            _progressRepository = progressRepository;
            _enrollmentRepository = enrollmentRepository;
        }

        [HttpGet]
        public IActionResult Dashboard()
        {
            var model = new AdminDashboardViewModel
            {
                TotalUsers = _userRepository.GetAll().Count(),
                TotalCourses = _courseRepository.GetAll().Count(),
                TotalLessons = _lessonRepository.GetLessonsByCourseId(null).Count(),
                TotalComments = _commentRepository.GetCommentsByCourse(null).Count(),
                TotalEnrollments = _enrollmentRepository.GetEnrollmentsByUser(null).Count(),
                TotalProgresses = _progressRepository.GetProgressByUser(null).Count()
            };

            return View(model); // Trả về Views/Admin/Dashboard.cshtml
        }

        // Các action quản lý người dùng
        [HttpGet]
        public IActionResult ManageUsers()
        {
            var users = _userRepository.GetAll();
            return View(users);
        }

        // Các action quản lý khóa học
        [HttpGet]
        public IActionResult ManageCourses()
        {
            var courses = _courseRepository.GetAllWithInstructor();
            return View(courses);
        }

        // Các action quản lý bài học
        [HttpGet]
        public IActionResult ManageLessons()
        {
            var lessons = _lessonRepository.GetLessonsByCourseId(null);
            return View(lessons);
        }

        // Các action quản lý bình luận
        [HttpGet]
        public IActionResult ManageComments()
        {
            var comments = _commentRepository.GetCommentsByCourse(null);
            return View(comments);
        }

        // Các action quản lý tiến độ
        [HttpGet]
        public IActionResult ManageProgress()
        {
            var progresses = _progressRepository.GetProgressByUser(null);
            return View(progresses);
        }

        [HttpGet]
        public IActionResult ViewProgress(string userId, string lessonId)
        {
            // Lấy thông tin bài học
            var lesson = _lessonRepository.GetById(lessonId);
            if (lesson == null)
            {
                return NotFound("Bài học không tồn tại.");
            }

            // Lấy tiến độ của người dùng cho bài học này
            var progress = _progressRepository.GetProgressByUserAndCourse(userId, lesson.CourseId)
                                             .FirstOrDefault(p => p.LessonId == lessonId);

            // Tạo ViewModel để hiển thị thông tin
            var viewModel = new AdminProgressViewModel
            {
                UserId = userId,
                Lesson = lesson,
                Progress = progress
            };

            return View(viewModel); // Trả về Views/Admin/ViewProgress.cshtml
        }

        // Các action quản lý đăng ký
        [HttpGet]
        public IActionResult ManageEnrollments()
        {
            var enrollments = _enrollmentRepository.GetEnrollmentsByUser(null);
            return View(enrollments);
        }
    }
}