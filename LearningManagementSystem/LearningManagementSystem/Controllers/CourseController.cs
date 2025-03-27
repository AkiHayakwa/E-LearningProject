using LearningManagementSystem.Models.ViewModels;
using LearningManagementSystem.Models;
using LearningManagementSystem.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

public class CourseController : Controller
{
    private readonly ICourseRepository _courseRepository;
    private readonly ICommentRepository _commentRepository;
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly ILogger<CourseController> _logger;

    public CourseController(
        ICourseRepository courseRepository,
        ICommentRepository commentRepository,
        IEnrollmentRepository enrollmentRepository,
        ILogger<CourseController> logger)
    {
        _courseRepository = courseRepository;
        _commentRepository = commentRepository;
        _enrollmentRepository = enrollmentRepository;
        _logger = logger;
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

        var comments = _commentRepository.GetAll()
            .Where(c => c.CourseId == id)
            .OrderByDescending(c => c.CreatedDate)
            .ToList();

        var userName = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var isEnrolled = userName != null && _enrollmentRepository.GetAll()
            .Any(e => e.UserName == userName && e.CourseId == id);

        var viewModel = new CourseDetailsViewModel
        {
            Course = course,
            Comments = comments,
            IsEnrolled = isEnrolled
        };

        return View(viewModel);
    }

    // GET: Course/ManageCourses
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