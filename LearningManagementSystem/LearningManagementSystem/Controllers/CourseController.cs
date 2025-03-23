using LearningManagementSystem.Models;
using LearningManagementSystem.Models.ViewModels;
using LearningManagementSystem.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using Microsoft.Extensions.Logging;

[Authorize]
public class CourseController : Controller
{
    private readonly ICourseRepository _courseRepository;
    private readonly ILessonRepository _lessonRepository;
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly ICommentRepository _commentRepository;
    private readonly IUserRepository _userRepository;
    private readonly IProgressRepository _progressRepository;
    private readonly ILogger<CourseController> _logger;

    public CourseController(
        ICourseRepository courseRepository,
        ILessonRepository lessonRepository,
        IEnrollmentRepository enrollmentRepository,
        ICommentRepository commentRepository,
        IUserRepository userRepository,
        IProgressRepository progressRepository,
        ILogger<CourseController> logger)
    {
        _courseRepository = courseRepository;
        _lessonRepository = lessonRepository;
        _enrollmentRepository = enrollmentRepository;
        _commentRepository = commentRepository;
        _userRepository = userRepository;
        _progressRepository = progressRepository;
        _logger = logger;
    }

    public IActionResult Index()
    {
        var courses = _courseRepository.GetAll();
        return View(courses);
    }

    [HttpGet]
    public IActionResult Details(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return NotFound();
        }

        var course = _courseRepository.GetById(id);
        if (course == null)
        {
            return NotFound();
        }

        return View(course);
    }

    [HttpGet]
    public IActionResult GetCourseDetails(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return NotFound();
        }

        var course = _courseRepository.GetById(id);
        if (course == null)
        {
            return NotFound();
        }

        return Json(new
        {
            title = course.CourseName,
            description = course.Description,
            instructor = course.Instructor?.FullName,
            createdDate = course.CreatedDate
        });
    }

    [HttpGet]
    public IActionResult EnrollCourse(string courseId, string lessonId)
    {
        _logger.LogInformation($"EnrollCourse called with CourseId: {courseId}, LessonId: {lessonId}");

        if (string.IsNullOrWhiteSpace(courseId))
        {
            _logger.LogWarning("CourseId is empty or null.");
            return NotFound();
        }

        var course = _courseRepository.GetById(courseId);
        if (course == null)
        {
            _logger.LogError($"Course with CourseId: {courseId} not found.");
            return NotFound();
        }

        var lessons = _lessonRepository.GetAll().Where(l => l.CourseId == courseId).OrderBy(l => l.OrderNumber).ToList();
        if (!lessons.Any())
        {
            TempData["Error"] = "Khóa học này chưa có bài học.";
            _logger.LogWarning($"No lessons found for CourseId: {courseId}.");
            return RedirectToAction("Index");
        }

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            TempData["Error"] = "Không thể xác định người dùng.";
            _logger.LogError("UserId could not be determined from claims.");
            return RedirectToAction("Index");
        }

        // Kiểm tra xem người dùng đã đăng ký khóa học chưa
        var enrollment = _enrollmentRepository.GetAll()
            .FirstOrDefault(e => e.UserId == userId && e.CourseId == courseId);
        if (enrollment == null)
        {
            // Tự động đăng ký khóa học cho người dùng
            enrollment = new Enrollment
            {
                EnrollmentId = Guid.NewGuid().ToString(),
                UserId = userId,
                CourseId = courseId,
                EnrollmentDate = DateTime.Now
            };
            _enrollmentRepository.Add(enrollment);
            _enrollmentRepository.Save();
            TempData["Success"] = "Đăng ký khóa học thành công!";
            _logger.LogInformation($"User {userId} enrolled in Course {courseId}.");
        }

        // Lấy bài học được chọn (nếu không có lessonId, lấy bài học đầu tiên)
        Lesson selectedLesson = null;
        if (!string.IsNullOrWhiteSpace(lessonId))
        {
            selectedLesson = lessons.FirstOrDefault(l => l.LessonId == lessonId);
            if (selectedLesson == null)
            {
                _logger.LogWarning($"Lesson with LessonId: {lessonId} not found in CourseId: {courseId}. Using first lesson instead.");
            }
        }
        if (selectedLesson == null)
        {
            selectedLesson = lessons.First();
            _logger.LogInformation($"No lesson selected. Defaulting to first lesson: {selectedLesson.LessonId}");
        }

        // Lấy bình luận cho bài học được chọn
        List<Comment> comments = new List<Comment>();
        if (selectedLesson != null)
        {
            comments = _commentRepository.GetAll()
                .Where(c => c.LessonId == selectedLesson.LessonId)
                .ToList();
        }
        else
        {
            _logger.LogWarning("SelectedLesson is null. No comments will be retrieved.");
        }

        // Lấy thông tin giảng viên
        User instructor = null;
        if (!string.IsNullOrEmpty(course.InstructorId))
        {
            instructor = _userRepository.GetById(course.InstructorId);
            if (instructor == null)
            {
                _logger.LogWarning($"Instructor with InstructorId: {course.InstructorId} not found.");
            }
        }

        var viewModel = new EnrollCourseViewModel
        {
            Course = course,
            Lessons = lessons,
            SelectedLesson = selectedLesson,
            Comments = comments,
            Instructor = instructor
        };

        return View(viewModel);
    }

    [HttpPost]
    public IActionResult AddComment(string lessonId, string content)
    {
        _logger.LogInformation($"AddComment called with LessonId: {lessonId}, Content: {content}");

        // Kiểm tra các trường cần thiết
        bool isValid = true;

        if (string.IsNullOrEmpty(lessonId))
        {
            ModelState.AddModelError("LessonId", "Bài học không được để trống.");
            isValid = false;
            _logger.LogWarning("LessonId is empty or null.");
        }
        else if (lessonId.Length > 50)
        {
            ModelState.AddModelError("LessonId", "Bài học không hợp lệ (quá dài).");
            isValid = false;
            _logger.LogWarning("LessonId exceeds 50 characters.");
        }

        if (string.IsNullOrEmpty(content))
        {
            ModelState.AddModelError("Content", "Nội dung bình luận không được để trống.");
            isValid = false;
            _logger.LogWarning("Content is empty or null.");
        }
        else if (content.Length > 1000)
        {
            ModelState.AddModelError("Content", "Nội dung bình luận không được dài quá 1000 ký tự.");
            isValid = false;
            _logger.LogWarning("Content exceeds 1000 characters.");
        }

        if (isValid)
        {
            try
            {
                // Lấy Lesson
                _logger.LogInformation($"Attempting to retrieve Lesson with LessonId: {lessonId}");
                var lesson = _lessonRepository.GetById(lessonId);
                if (lesson == null)
                {
                    _logger.LogError($"Lesson with LessonId: {lessonId} not found.");
                    TempData["Error"] = "Bài học không tồn tại.";
                    return RedirectToAction("Index");
                }

                // Lấy Course từ Lesson
                _logger.LogInformation($"Attempting to retrieve Course with CourseId: {lesson.CourseId}");
                var course = _courseRepository.GetById(lesson.CourseId);
                if (course == null)
                {
                    _logger.LogError($"Course with CourseId: {lesson.CourseId} not found.");
                    TempData["Error"] = "Khóa học không tồn tại.";
                    return RedirectToAction("Index");
                }

                // Lấy UserId từ thông tin đăng nhập
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogError("UserId could not be determined from claims.");
                    TempData["Error"] = "Không thể xác định người dùng.";
                    return RedirectToAction("Index");
                }
                _logger.LogInformation($"UserId retrieved: {userId}");

                // Kiểm tra xem người dùng đã đăng ký khóa học chưa
                _logger.LogInformation($"Checking enrollment for UserId: {userId}, CourseId: {lesson.CourseId}");
                var enrollment = _enrollmentRepository.GetAll()
                    .FirstOrDefault(e => e.UserId == userId && e.CourseId == lesson.CourseId);
                if (enrollment == null)
                {
                    _logger.LogError($"Enrollment not found for UserId: {userId}, CourseId: {lesson.CourseId}");
                    TempData["Error"] = "Bạn chưa đăng ký khóa học này.";
                    return RedirectToAction("Index");
                }

                // Tạo Comment mới
                var comment = new Comment
                {
                    CommentId = Guid.NewGuid().ToString(),
                    Content = content,
                    CreatedDate = DateTime.Now,
                    UserId = userId,
                    LessonId = lessonId,
                    CourseId = lesson.CourseId
                };

                _logger.LogInformation($"Adding new comment with CommentId: {comment.CommentId}");
                _commentRepository.Add(comment);
                _commentRepository.Save();
                TempData["Success"] = "Thêm bình luận thành công!";
                return RedirectToAction("EnrollCourse", new { courseId = lesson.CourseId, lessonId = lessonId });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred while adding comment: {ex.Message}");
                TempData["Error"] = $"Đã có lỗi xảy ra: {ex.Message}";
            }
        }
        else
        {
            // Hiển thị lỗi validation chi tiết
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            TempData["Error"] = "Dữ liệu không hợp lệ: " + string.Join(", ", errors);
            _logger.LogWarning($"Validation errors: {string.Join(", ", errors)}");
        }

        // Nếu có lỗi, quay lại trang EnrollCourse
        var redirectLesson = _lessonRepository.GetById(lessonId);
        if (redirectLesson != null)
        {
            return RedirectToAction("EnrollCourse", new { courseId = redirectLesson.CourseId, lessonId = lessonId });
        }
        return RedirectToAction("Index");
    }
}