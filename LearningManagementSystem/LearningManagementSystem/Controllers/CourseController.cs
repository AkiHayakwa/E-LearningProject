using LearningManagementSystem.Models;
using LearningManagementSystem.Models.ViewModels;
using LearningManagementSystem.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace LearningManagementSystem.Controllers
{
    public class CourseController : Controller
    {
        private readonly ICourseRepository _courseRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly IProgressRepository _progressRepository; // Thêm repository cho Progress
        private readonly ILogger<CourseController> _logger;

        public CourseController(
            ICourseRepository courseRepository,
            ICommentRepository commentRepository,
            IEnrollmentRepository enrollmentRepository,
            ILessonRepository lessonRepository,
            IProgressRepository progressRepository,
            ILogger<CourseController> logger)
        {
            _courseRepository = courseRepository;
            _commentRepository = commentRepository;
            _enrollmentRepository = enrollmentRepository;
            _lessonRepository = lessonRepository;
            _progressRepository = progressRepository;
            _logger = logger;
        }

        // GET: Course/Index
        public IActionResult Index()
        {
            _logger.LogInformation("Index called to display list of courses.");

            var userName = User.Identity.IsAuthenticated ? User.FindFirst(ClaimTypes.Name)?.Value : null;
            var courses = _courseRepository.GetAll()
                .Select(c => new CourseListViewModel
                {
                    CourseId = c.CourseId,
                    CourseName = c.CourseName,
                    Description = c.Description,
                    CreatedDate = c.CreatedDate,
                    ImageUrl = c.ImageUrl,
                    IsEnrolled = userName != null && _enrollmentRepository.GetAll()
                        .Any(e => e.UserName == userName && e.CourseId == c.CourseId)
                })
                .ToList();

            return View(courses);
        }

        // GET: Course/CourseDetails/{id}
        [Authorize]
        public IActionResult CourseDetails(string id)
        {
            _logger.LogInformation($"CourseDetails called with CourseId: {id}");

            var course = _courseRepository.GetById(id);
            if (course == null)
            {
                _logger.LogWarning($"Course with CourseId: {id} not found.");
                return NotFound();
            }

            // Lấy danh sách bài học
            var lessons = _lessonRepository.GetLessonsByCourse(id).ToList();
            course.Lessons = lessons;

            // Lấy danh sách bình luận
            var comments = _commentRepository.GetAll()
                .Where(c => c.CourseId == id)
                .OrderByDescending(c => c.CreatedDate)
                .ToList();

            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userName))
            {
                _logger.LogWarning("UserName could not be determined from claims.");
                return Unauthorized("Bạn cần đăng nhập để xem chi tiết khóa học.");
            }

            var isEnrolled = _enrollmentRepository.GetAll()
                .Any(e => e.UserName == userName && e.CourseId == id);

            // Lấy danh sách tiến trình của user trong khóa học
            var progresses = _progressRepository.GetAll()
                .Where(p => p.UserName == userName && p.Lesson != null && p.Lesson.CourseId == id)
                .ToList();

            var viewModel = new CourseDetailsViewModel
            {
                Course = course,
                Comments = comments,
                IsEnrolled = isEnrolled,
                Progresses = progresses // Thêm Progresses vào ViewModel
            };

            _logger.LogInformation($"Found {progresses.Count} progress records for user {userName} in course {id}");
            return View(viewModel);
        }

        // POST: Course/Enroll/{id}
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Enroll(string id)
        {
            _logger.LogInformation($"Enroll called with CourseId: {id}");

            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userName))
            {
                _logger.LogWarning("UserName could not be determined from claims.");
                return Unauthorized("Bạn cần đăng nhập để đăng ký khóa học.");
            }

            var course = _courseRepository.GetById(id);
            if (course == null)
            {
                _logger.LogWarning($"Course with CourseId: {id} not found.");
                return NotFound("Không tìm thấy khóa học.");
            }

            if (!_enrollmentRepository.GetAll().Any(e => e.UserName == userName && e.CourseId == id))
            {
                var enrollment = new Enrollment
                {
                    EnrollmentId = Guid.NewGuid().ToString(),
                    UserName = userName,
                    CourseId = id,
                    EnrollmentDate = DateTime.Now
                };
                _enrollmentRepository.Add(enrollment);
                _enrollmentRepository.Save();
                _logger.LogInformation($"User {userName} enrolled in CourseId: {id}");
            }

            return RedirectToAction("CourseDetails", new { id });
        }

        [Authorize(Roles = "Admin")]
        public IActionResult ManageCourses()
        {
            _logger.LogInformation("ManageCourses called.");

            var courses = _courseRepository.GetAll().ToList();
            return View(courses);
        }

        // GET: Course/CreateCourse
        [Authorize(Roles = "Admin")]
        public IActionResult CreateCourse()
        {
            _logger.LogInformation("CreateCourse GET called.");
            return View();
        }

        // POST: Course/CreateCourse
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateCourse(Course model, IFormFile imageFile)
        {
            _logger.LogInformation("CreateCourse POST called.");

            if (ModelState.IsValid)
            {
                try
                {
                    model.CourseId = Guid.NewGuid().ToString();
                    model.CreatedDate = DateTime.Now;

                    // Xử lý upload hình ảnh
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }
                        model.ImageUrl = $"/images/{fileName}";
                    }

                    _courseRepository.Add(model);
                    _courseRepository.Save();
                    TempData["Success"] = "Thêm khóa học thành công.";
                    _logger.LogInformation($"Course {model.CourseId} created successfully.");
                    return RedirectToAction("ManageCourses");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error creating course: {model.CourseName}");
                    ModelState.AddModelError("", "Đã xảy ra lỗi khi thêm khóa học. Vui lòng thử lại.");
                }
            }
            return View(model);
        }

        // GET: Course/EditCourse/{id}
        [Authorize(Roles = "Admin")]
        public IActionResult EditCourse(string id)
        {
            _logger.LogInformation($"EditCourse GET called with CourseId: {id}");

            var course = _courseRepository.GetById(id);
            if (course == null)
            {
                _logger.LogWarning($"Course with CourseId: {id} not found.");
                return NotFound();
            }
            return View(course);
        }

        // POST: Course/EditCourse/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditCourse(string id, Course model, IFormFile imageFile)
        {
            _logger.LogInformation($"EditCourse POST called with CourseId: {id}");

            if (id != model.CourseId)
            {
                _logger.LogWarning($"CourseId mismatch: {id} != {model.CourseId}");
                return NotFound();
            }

            var course = _courseRepository.GetById(id);
            if (course == null)
            {
                _logger.LogWarning($"Course with CourseId: {id} not found.");
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    course.CourseName = model.CourseName;
                    course.Description = model.Description;

                    // Xử lý upload hình ảnh mới (nếu có)
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        // Xóa hình ảnh cũ (nếu có)
                        if (!string.IsNullOrEmpty(course.ImageUrl))
                        {
                            var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", course.ImageUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        // Upload hình ảnh mới
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }
                        course.ImageUrl = $"/images/{fileName}";
                    }

                    _courseRepository.Update(course);
                    _courseRepository.Save();
                    TempData["Success"] = "Chỉnh sửa khóa học thành công.";
                    _logger.LogInformation($"Course {course.CourseId} updated successfully.");
                    return RedirectToAction("ManageCourses");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error updating course: {course.CourseId}");
                    ModelState.AddModelError("", "Đã xảy ra lỗi khi chỉnh sửa khóa học. Vui lòng thử lại.");
                }
            }
            return View(model);
        }

        // POST: Course/DeleteCourse/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteCourse(string id)
        {
            _logger.LogInformation($"DeleteCourse called with CourseId: {id}");

            var course = _courseRepository.GetById(id);
            if (course == null)
            {
                _logger.LogWarning($"Course with CourseId: {id} not found.");
                TempData["Error"] = "Khóa học không tồn tại.";
                return RedirectToAction("ManageCourses");
            }

            try
            {
                // Xóa các bình luận liên quan
                var comments = _commentRepository.GetAll().Where(c => c.CourseId == id).ToList();
                foreach (var comment in comments)
                {
                    _commentRepository.Delete(comment.CommentId);
                }

                // Xóa các ghi danh liên quan
                var enrollments = _enrollmentRepository.GetAll().Where(e => e.CourseId == id).ToList();
                foreach (var enrollment in enrollments)
                {
                    _enrollmentRepository.Delete(enrollment.EnrollmentId);
                }

                // Xóa hình ảnh (nếu có)
                if (!string.IsNullOrEmpty(course.ImageUrl))
                {
                    var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", course.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                _courseRepository.Delete(id);
                _courseRepository.Save();
                TempData["Success"] = "Xóa khóa học thành công.";
                _logger.LogInformation($"Course {id} deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting course: {id}");
                TempData["Error"] = "Đã xảy ra lỗi khi xóa khóa học. Vui lòng thử lại.";
            }

            return RedirectToAction("ManageCourses");
        }
    }
}