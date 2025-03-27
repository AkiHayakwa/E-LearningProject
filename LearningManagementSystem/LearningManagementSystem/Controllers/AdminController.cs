using LearningManagementSystem.Data;
using LearningManagementSystem.Models;
using LearningManagementSystem.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Threading.Tasks;

namespace LearningManagementSystem.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ICourseRepository _courseRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly IUserRepository _userRepository;
        private readonly LMSContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            ICourseRepository courseRepository,
            ICommentRepository commentRepository,
            IEnrollmentRepository enrollmentRepository,
            IUserRepository userRepository,
            LMSContext context,
            IPasswordHasher<User> passwordHasher,
            ILogger<AdminController> logger)
        {
            _courseRepository = courseRepository;
            _commentRepository = commentRepository;
            _enrollmentRepository = enrollmentRepository;
            _userRepository = userRepository;
            _context = context;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        #region Quản lý khóa học (Course Management)

        // GET: Admin/ManageCourses
        public IActionResult ManageCourses()
        {
            _logger.LogInformation("ManageCourses called.");
            var courses = _courseRepository.GetAll().ToList();
            return View(courses);
        }

        // GET: Admin/CreateCourse
        public IActionResult CreateCourse()
        {
            _logger.LogInformation("CreateCourse GET called.");
            return View();
        }

        // POST: Admin/CreateCourse
        [HttpPost]
        [ValidateAntiForgeryToken]
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

        // GET: Admin/EditCourse/{id}
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

        // POST: Admin/EditCourse/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
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

        // POST: Admin/DeleteCourse/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
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

        #endregion

        #region Quản lý người dùng (User Management)

        // GET: Admin/ManageUsers
        public IActionResult ManageUsers()
        {
            try
            {
                var users = _userRepository.GetAll().ToList();
                return View(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users in ManageUsers.");
                TempData["Error"] = "Đã xảy ra lỗi khi lấy danh sách người dùng. Vui lòng thử lại.";
                return RedirectToAction("ManageCourses");
            }
        }

        // GET: Admin/CreateUser
        public IActionResult CreateUser()
        {
            try
            {
                ViewBag.Roles = _context.Roles.ToList();
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading roles in CreateUser.");
                TempData["Error"] = "Đã xảy ra lỗi khi tải danh sách vai trò. Vui lòng thử lại.";
                return RedirectToAction("ManageUsers");
            }
        }

        // POST: Admin/CreateUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateUser(User model, string password)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var existingUser = _userRepository.GetByUserName(model.UserName);
                    if (existingUser != null)
                    {
                        ModelState.AddModelError("UserName", "Tên đăng nhập đã tồn tại.");
                        ViewBag.Roles = _context.Roles.ToList();
                        return View(model);
                    }

                    if (string.IsNullOrEmpty(password))
                    {
                        ModelState.AddModelError("Password", "Mật khẩu không được để trống.");
                        ViewBag.Roles = _context.Roles.ToList();
                        return View(model);
                    }

                    model.HashPassword(_passwordHasher, password);
                    _userRepository.Add(model);
                    TempData["Success"] = "Thêm người dùng thành công.";
                    return RedirectToAction("ManageUsers");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error creating user: {model.UserName}");
                    ModelState.AddModelError("", "Đã xảy ra lỗi khi thêm người dùng. Vui lòng thử lại.");
                }
            }
            ViewBag.Roles = _context.Roles.ToList();
            return View(model);
        }

        // GET: Admin/EditUser/{userName}
        public IActionResult EditUser(string userName)
        {
            if (string.IsNullOrEmpty(userName))
            {
                _logger.LogWarning("EditUser called with null or empty userName.");
                return NotFound();
            }

            try
            {
                var user = _userRepository.GetByUserName(userName);
                if (user == null)
                {
                    _logger.LogWarning($"User with UserName: {userName} not found.");
                    return NotFound();
                }
                ViewBag.Roles = _context.Roles.ToList();
                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving user: {userName}");
                TempData["Error"] = "Đã xảy ra lỗi khi lấy thông tin người dùng. Vui lòng thử lại.";
                return RedirectToAction("ManageUsers");
            }
        }

        // POST: Admin/EditUser/{userName}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditUser(string userName, User model, string password)
        {
            if (string.IsNullOrEmpty(userName) || userName != model.UserName)
            {
                _logger.LogWarning($"UserName mismatch: {userName} != {model.UserName}");
                return NotFound();
            }

            try
            {
                var user = _userRepository.GetByUserName(userName);
                if (user == null)
                {
                    _logger.LogWarning($"User with UserName: {userName} not found.");
                    return NotFound();
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        user.FullName = model.FullName;
                        user.Email = model.Email;
                        user.RoleId = model.RoleId;

                        if (!string.IsNullOrEmpty(password))
                        {
                            user.HashPassword(_passwordHasher, password);
                        }

                        _userRepository.Update(user);
                        TempData["Success"] = "Chỉnh sửa người dùng thành công.";
                        return RedirectToAction("ManageUsers");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error updating user: {userName}");
                        ModelState.AddModelError("", "Đã xảy ra lỗi khi chỉnh sửa người dùng. Vui lòng thử lại.");
                    }
                }
                ViewBag.Roles = _context.Roles.ToList();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving user for edit: {userName}");
                TempData["Error"] = "Đã xảy ra lỗi khi lấy thông tin người dùng. Vui lòng thử lại.";
                return RedirectToAction("ManageUsers");
            }
        }

        // POST: Admin/DeleteUser/{userName}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteUser(string userName)
        {
            if (string.IsNullOrEmpty(userName))
            {
                _logger.LogWarning("DeleteUser called with null or empty userName.");
                TempData["Error"] = "Tên đăng nhập không hợp lệ.";
                return RedirectToAction("ManageUsers");
            }

            try
            {
                var user = _userRepository.GetByUserName(userName);
                if (user == null)
                {
                    _logger.LogWarning($"User with UserName: {userName} not found.");
                    TempData["Error"] = "Người dùng không tồn tại.";
                    return RedirectToAction("ManageUsers");
                }

                try
                {
                    _userRepository.Delete(userName);
                    TempData["Success"] = "Xóa người dùng thành công.";
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error deleting user: {userName}");
                    TempData["Error"] = "Đã xảy ra lỗi khi xóa người dùng. Vui lòng thử lại.";
                }

                return RedirectToAction("ManageUsers");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving user for delete: {userName}");
                TempData["Error"] = "Đã xảy ra lỗi khi lấy thông tin người dùng. Vui lòng thử lại.";
                return RedirectToAction("ManageUsers");
            }
        }

        #endregion
    }
}