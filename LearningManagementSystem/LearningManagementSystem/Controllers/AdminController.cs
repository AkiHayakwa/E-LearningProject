using LearningManagementSystem.Data;
using LearningManagementSystem.Models;
using LearningManagementSystem.Models.ViewModels;
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
        private readonly ILessonRepository _lessonRepository;
        private readonly LMSContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            ICourseRepository courseRepository,
            ICommentRepository commentRepository,
            IEnrollmentRepository enrollmentRepository,
            IUserRepository userRepository,
            ILessonRepository lessonRepository,
            LMSContext context,
            IPasswordHasher<User> passwordHasher,
            ILogger<AdminController> logger)
        {
            _courseRepository = courseRepository;
            _commentRepository = commentRepository;
            _enrollmentRepository = enrollmentRepository;
            _userRepository = userRepository;
            _lessonRepository = lessonRepository;
            _context = context;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        #region Dashboard

        // GET: Admin/Dashboard
        public IActionResult Dashboard()
        {
            try
            {
                var viewModel = new AdminDashboardViewModel
                {
                    TotalUsers = _userRepository.GetAll().Count(),
                    TotalCourses = _courseRepository.GetAll().Count(),
                    TotalComments = _commentRepository.GetAll().Count(),
                    TotalEnrollments = _enrollmentRepository.GetAll().Count()
                };

                return View("~/Views/Admin/Dashboard.cshtml", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Admin Dashboard.");
                TempData["Error"] = "Đã xảy ra lỗi khi tải trang Dashboard. Vui lòng thử lại.";
                return RedirectToAction("ManageCourses");
            }
        }

        #endregion

        #region Quản lý khóa học (Course Management)

        // GET: Admin/ManageCourses
        public IActionResult ManageCourses()
        {
            _logger.LogInformation("ManageCourses called.");
            var courses = _courseRepository.GetAll().ToList();
            return View("~/Views/Admin/Course/ManageCourses.cshtml", courses);
        }

        // GET: Admin/CreateCourse
        public IActionResult CreateCourse()
        {
            _logger.LogInformation("CreateCourse GET called.");
            return View("~/Views/Admin/Course/CreateCourse.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCourse(Course model, IFormFile imageFile)
        {
            _logger.LogInformation("CreateCourse POST called.");

            // Gán các giá trị trước khi kiểm tra ModelState
            model.CourseId = Guid.NewGuid().ToString();
            model.CreatedDate = DateTime.Now;

            // Xử lý upload hình ảnh trước khi kiểm tra ModelState
            if (imageFile != null && imageFile.Length > 0)
            {
                var imageDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                if (!Directory.Exists(imageDirectory))
                {
                    Directory.CreateDirectory(imageDirectory);
                }

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                var filePath = Path.Combine(imageDirectory, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }
                model.ImageUrl = $"/images/{fileName}";
            }
            else
            {
                model.ImageUrl = null; // Đảm bảo ImageUrl là null nếu không có hình ảnh
            }

            // Xóa ModelState cũ và tái xác thực
            ModelState.Clear();
            TryValidateModel(model);

            if (ModelState.IsValid)
            {
                try
                {
                    _courseRepository.Add(model);
                    _courseRepository.Save();
                    TempData["Success"] = "Thêm khóa học thành công. Bạn có thể thêm bài học ngay bây giờ.";
                    _logger.LogInformation($"Course {model.CourseId} created successfully.");

                    return RedirectToAction("EditCourse", new { id = model.CourseId });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error creating course: {model.CourseName}");
                    ModelState.AddModelError("", $"Đã xảy ra lỗi khi thêm khóa học: {ex.Message}");
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                _logger.LogWarning("ModelState is invalid in CreateCourse. Errors: {0}", string.Join(", ", errors));
            }

            return View("~/Views/Admin/Course/CreateCourse.cshtml", model);
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

            // Lấy danh sách bài học của khóa học
            course.Lessons = _lessonRepository.GetLessonsByCourse(id).ToList();
            return View("~/Views/Admin/Course/EditCourse.cshtml", course);
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

            // Nếu có lỗi, cần tải lại danh sách bài học
            course.Lessons = _lessonRepository.GetLessonsByCourse(id).ToList();
            return View("~/Views/Admin/Course/EditCourse.cshtml", model);
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
                // Xóa các bài học liên quan
                var lessons = _lessonRepository.GetLessonsByCourse(id).ToList();
                foreach (var lesson in lessons)
                {
                    _lessonRepository.Delete(lesson.LessonId);
                }
                _lessonRepository.Save();

                // Xóa các bình luận liên quan
                var comments = _commentRepository.GetAll().Where(c => c.CourseId == id).ToList();
                foreach (var comment in comments)
                {
                    _commentRepository.Delete(comment.CommentId);
                }
                _commentRepository.Save();

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

        #region Quản lý bài học trong EditCourse

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddLessonInEditCourse(string courseId, Lesson lesson)
        {
            _logger.LogInformation($"AddLessonInEditCourse POST called for CourseId: {courseId}");

            // Gán các giá trị trước khi kiểm tra ModelState
            lesson.LessonId = Guid.NewGuid().ToString();
            lesson.CourseId = courseId;

            // Gán OrderNumber (nếu không được gửi từ form)
            if (lesson.OrderNumber == 0)
            {
                lesson.OrderNumber = _lessonRepository.GetLessonsByCourse(courseId).Count() + 1;
            }

            // Khởi tạo Progresses (danh sách rỗng) để tránh lỗi
            lesson.Progresses = lesson.Progresses ?? new List<Progress>();

            // Xóa ModelState cũ và tái xác thực
            ModelState.Clear();
            TryValidateModel(lesson);

            if (ModelState.IsValid)
            {
                try
                {
                    _lessonRepository.Add(lesson);
                    _lessonRepository.Save();
                    TempData["Success"] = "Thêm bài học thành công.";
                    _logger.LogInformation($"Lesson {lesson.LessonId} added successfully to Course {courseId}.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error adding lesson to course: {courseId}");
                    ModelState.AddModelError("", $"Đã xảy ra lỗi khi thêm bài học: {ex.Message}");
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                _logger.LogWarning("ModelState is invalid in AddLessonInEditCourse. Errors: {0}", string.Join(", ", errors));
            }

            // Tải lại dữ liệu khóa học để hiển thị trong view
            var course = _courseRepository.GetById(courseId);
            if (course == null)
            {
                _logger.LogWarning($"Course with CourseId: {courseId} not found.");
                return NotFound();
            }
            course.Lessons = _lessonRepository.GetLessonsByCourse(courseId).ToList();

            // Truyền dữ liệu bài học đã nhập vào TempData để giữ lại khi có lỗi
            if (!ModelState.IsValid)
            {
                TempData["LessonTitle"] = lesson.LessonTitle;
                TempData["Content"] = lesson.Content;
                TempData["LinkYoutube"] = lesson.LinkYoutube;
                TempData["OrderNumber"] = lesson.OrderNumber.ToString();
            }

            return View("~/Views/Admin/Course/EditCourse.cshtml", course);
        }

        // POST: Admin/EditLessonInEditCourse/{lessonId}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditLessonInEditCourse(string lessonId, Lesson model)
        {
            _logger.LogInformation($"EditLessonInEditCourse POST called for LessonId: {lessonId}");

            var lesson = _lessonRepository.GetById(lessonId);
            if (lesson == null)
            {
                _logger.LogWarning($"Lesson with LessonId: {lessonId} not found.");
                TempData["Error"] = "Bài học không tồn tại.";
                return RedirectToAction("ManageCourses");
            }

            // Cập nhật các giá trị từ model
            lesson.LessonTitle = model.LessonTitle;
            lesson.Content = model.Content;
            lesson.LinkYoutube = model.LinkYoutube;
            lesson.OrderNumber = model.OrderNumber;

            // Đảm bảo CourseId không bị thay đổi
            lesson.CourseId = lesson.CourseId; // Giữ nguyên CourseId từ bản ghi hiện tại

            // Khởi tạo Progresses (danh sách rỗng) nếu cần
            lesson.Progresses = lesson.Progresses ?? new List<Progress>();

            // Xóa ModelState cũ và tái xác thực
            ModelState.Clear();
            TryValidateModel(lesson);

            if (ModelState.IsValid)
            {
                try
                {
                    _lessonRepository.Update(lesson);
                    _lessonRepository.Save();
                    TempData["Success"] = "Chỉnh sửa bài học thành công.";
                    _logger.LogInformation($"Lesson {lesson.LessonId} updated successfully.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error updating lesson: {lessonId}");
                    ModelState.AddModelError("", $"Đã xảy ra lỗi khi chỉnh sửa bài học: {ex.Message}");
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                _logger.LogWarning("ModelState is invalid in EditLessonInEditCourse. Errors: {0}", string.Join(", ", errors));
            }

            // Tải lại dữ liệu khóa học để hiển thị trong view
            var course = _courseRepository.GetById(lesson.CourseId);
            if (course == null)
            {
                _logger.LogWarning($"Course with CourseId: {lesson.CourseId} not found.");
                return NotFound();
            }
            course.Lessons = _lessonRepository.GetLessonsByCourse(lesson.CourseId).ToList();

            // Truyền dữ liệu bài học đã nhập vào TempData để giữ lại khi có lỗi
            if (!ModelState.IsValid)
            {
                TempData["LessonTitle"] = lesson.LessonTitle;
                TempData["Content"] = lesson.Content;
                TempData["LinkYoutube"] = lesson.LinkYoutube;
                TempData["OrderNumber"] = lesson.OrderNumber.ToString();
            }

            return View("~/Views/Admin/Course/EditCourse.cshtml", course);
        }

        // POST: Admin/DeleteLessonInEditCourse/{lessonId}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteLessonInEditCourse(string lessonId)
        {
            var lesson = _lessonRepository.GetById(lessonId);
            if (lesson == null)
            {
                TempData["Error"] = "Bài học không tồn tại.";
                return RedirectToAction("ManageCourses");
            }

            try
            {
                var courseId = lesson.CourseId;
                _lessonRepository.Delete(lessonId);
                _lessonRepository.Save();

                // Cập nhật lại OrderNumber của các bài học còn lại
                var remainingLessons = _lessonRepository.GetLessonsByCourse(courseId).ToList();
                for (int i = 0; i < remainingLessons.Count; i++)
                {
                    remainingLessons[i].OrderNumber = i + 1;
                    _lessonRepository.Update(remainingLessons[i]);
                }
                _lessonRepository.Save();

                TempData["Success"] = "Xóa bài học thành công.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting lesson: {lessonId}");
                TempData["Error"] = "Đã xảy ra lỗi khi xóa bài học. Vui lòng thử lại.";
            }
            return RedirectToAction("EditCourse", new { id = lesson.CourseId });
        }

        #endregion

        #endregion

        #region Quản lý bình luận (Comment Management)

        public IActionResult ManageComments()
        {
            _logger.LogInformation("ManageComments called.");
            var comments = _commentRepository.GetAll().ToList();
            return View("~/Views/Admin/Comment/ManageComments.cshtml", comments);
        }

        // POST: Admin/DeleteComment/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteComment(string id)
        {
            _logger.LogInformation($"DeleteComment called with CommentId: {id}");

            var comment = _commentRepository.GetById(id);
            if (comment == null)
            {
                _logger.LogWarning($"Comment with CommentId: {id} not found.");
                TempData["Error"] = "Bình luận không tồn tại.";
                return RedirectToAction("ManageComments");
            }

            try
            {
                _commentRepository.Delete(id);
                _commentRepository.Save();
                TempData["Success"] = "Xóa bình luận thành công.";
                _logger.LogInformation($"Comment {id} deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting comment: {id}");
                TempData["Error"] = "Đã xảy ra lỗi khi xóa bình luận. Vui lòng thử lại.";
            }

            return RedirectToAction("ManageComments");
        }

        #endregion

        #region Quản lý người dùng (User Management)

        // GET: Admin/ManageUsers
        public IActionResult ManageUsers()
        {
            try
            {
                var users = _userRepository.GetAll().ToList();
                return View("~/Views/Admin/User/ManageUsers.cshtml", users);
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
                return View("~/Views/Admin/User/CreateUser.cshtml");
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
            // Gán giá trị tạm thời cho Password để vượt qua kiểm tra ModelState
            model.Password = password ?? string.Empty;

            if (ModelState.IsValid)
            {
                try
                {
                    var existingUser = _userRepository.GetByUserName(model.UserName);
                    if (existingUser != null)
                    {
                        ModelState.AddModelError("UserName", "Tên đăng nhập đã tồn tại.");
                        ViewBag.Roles = _context.Roles.ToList();
                        return View("~/Views/Admin/User/CreateUser.cshtml", model);
                    }

                    if (string.IsNullOrEmpty(password))
                    {
                        ModelState.AddModelError("Password", "Mật khẩu không được để trống.");
                        ViewBag.Roles = _context.Roles.ToList();
                        return View("~/Views/Admin/User/CreateUser.cshtml", model);
                    }

                    // Kiểm tra RoleId có hợp lệ không
                    var role = _context.Roles.FirstOrDefault(r => r.RoleId == model.RoleId);
                    if (role == null)
                    {
                        ModelState.AddModelError("RoleId", "Vai trò không hợp lệ.");
                        ViewBag.Roles = _context.Roles.ToList();
                        return View("~/Views/Admin/User/CreateUser.cshtml", model);
                    }

                    model.HashPassword(_passwordHasher, password);
                    _userRepository.Add(model);
                    _userRepository.Save();
                    TempData["Success"] = "Thêm người dùng thành công.";
                    return RedirectToAction("ManageUsers");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error creating user: {model.UserName}");
                    ModelState.AddModelError("", "Đã xảy ra lỗi khi thêm người dùng. Vui lòng thử lại.");
                }
            }
            else
            {
                // Ghi log các lỗi trong ModelState
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                _logger.LogWarning("ModelState is invalid in CreateUser. Errors: {0}", string.Join(", ", errors));
            }
            ViewBag.Roles = _context.Roles.ToList();
            return View("~/Views/Admin/User/CreateUser.cshtml", model);
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
                return View("~/Views/Admin/User/EditUser.cshtml", user);
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

                // Gán giá trị tạm thời cho Password để vượt qua kiểm tra ModelState
                model.Password = user.Password; // Giữ nguyên mật khẩu cũ nếu không thay đổi

                if (ModelState.IsValid)
                {
                    try
                    {
                        // Kiểm tra RoleId có hợp lệ không
                        var role = _context.Roles.FirstOrDefault(r => r.RoleId == model.RoleId);
                        if (role == null)
                        {
                            ModelState.AddModelError("RoleId", "Vai trò không hợp lệ.");
                            ViewBag.Roles = _context.Roles.ToList();
                            return View("~/Views/Admin/User/EditUser.cshtml", model);
                        }

                        // Cập nhật thông tin người dùng
                        user.FullName = model.FullName;
                        user.Email = model.Email;
                        user.RoleId = model.RoleId;

                        // Nếu có mật khẩu mới, băm và cập nhật
                        if (!string.IsNullOrEmpty(password))
                        {
                            user.HashPassword(_passwordHasher, password);
                        }

                        _userRepository.Update(user);
                        _userRepository.Save();
                        TempData["Success"] = "Chỉnh sửa người dùng thành công.";
                        return RedirectToAction("ManageUsers");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error updating user: {userName}");
                        ModelState.AddModelError("", "Đã xảy ra lỗi khi chỉnh sửa người dùng. Vui lòng thử lại.");
                    }
                }
                else
                {
                    // Ghi log các lỗi trong ModelState
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    _logger.LogWarning("ModelState is invalid in EditUser. Errors: {0}", string.Join(", ", errors));
                }
                ViewBag.Roles = _context.Roles.ToList();
                return View("~/Views/Admin/User/EditUser.cshtml", model);
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
                    _userRepository.Save(); // Thêm dòng này để lưu thay đổi vào cơ sở dữ liệu
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