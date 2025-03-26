using LearningManagementSystem.Data;
using LearningManagementSystem.Models;
using LearningManagementSystem.Models.ViewModels;
using LearningManagementSystem.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ICourseRepository _courseRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILessonRepository _lessonRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly ICommentRepository _commentRepository;
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly IProgressRepository _progressRepository;
    private readonly IPasswordHasher<User> _passwordHasher;

    public AdminController(
        ICourseRepository courseRepository,
        IUserRepository userRepository,
        ILessonRepository lessonRepository,
        IRoleRepository roleRepository,
        ICommentRepository commentRepository,
        IEnrollmentRepository enrollmentRepository,
        IProgressRepository progressRepository,
        IPasswordHasher<User> passwordHasher)
    {
        _courseRepository = courseRepository;
        _userRepository = userRepository;
        _lessonRepository = lessonRepository;
        _roleRepository = roleRepository;
        _commentRepository = commentRepository;
        _enrollmentRepository = enrollmentRepository;
        _progressRepository = progressRepository;
        _passwordHasher = passwordHasher;
    }

    public IActionResult Dashboard()
    {
        try
        {
            var model = new AdminDashboardViewModel
            {
                TotalUsers = _userRepository.GetAll().Count(),
                TotalCourses = _courseRepository.GetAll().Count(),
                TotalLessons = _lessonRepository.GetAll().Count(),
                TotalComments = _commentRepository.GetAll().Count(),
                TotalEnrollments = _enrollmentRepository.GetAll().Count(),
                TotalProgresses = _progressRepository.GetAll().Count()
            };
            return View(model);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Đã có lỗi xảy ra: {ex.Message}";
            return RedirectToAction("Index", "Home");
        }
    }

    // Hiển thị form thêm người dùng
    [HttpGet]
    public IActionResult AddUser()
    {
        try
        {
            var roles = _roleRepository.GetAll().Where(r => r.RoleName != "Instructor").ToList();
            if (!roles.Any())
            {
                TempData["Error"] = "Không có vai trò nào trong hệ thống. Vui lòng thêm vai trò trước.";
                return RedirectToAction("ManageUsers");
            }
            ViewBag.Roles = roles;
            return View(new User());
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Đã có lỗi xảy ra: {ex.Message}";
            return RedirectToAction("ManageUsers");
        }
    }

    [HttpPost]
    public IActionResult AddUser(User user, string password)
    {
        // Kiểm tra các trường cần thiết
        bool isValid = true;

        if (string.IsNullOrEmpty(user.UserName))
        {
            ModelState.AddModelError("UserName", "Tên đăng nhập không được để trống.");
            isValid = false;
        }
        else if (user.UserName.Length > 50)
        {
            ModelState.AddModelError("UserName", "Tên đăng nhập không được dài quá 50 ký tự.");
            isValid = false;
        }

        if (string.IsNullOrEmpty(password))
        {
            ModelState.AddModelError("password", "Mật khẩu không được để trống.");
            isValid = false;
        }

        if (string.IsNullOrEmpty(user.Email))
        {
            ModelState.AddModelError("Email", "Email không được để trống.");
            isValid = false;
        }
        else if (!new EmailAddressAttribute().IsValid(user.Email))
        {
            ModelState.AddModelError("Email", "Email không hợp lệ.");
            isValid = false;
        }

        if (string.IsNullOrEmpty(user.RoleId))
        {
            ModelState.AddModelError("RoleId", "Vai trò không được để trống.");
            isValid = false;
        }

        if (user.FullName != null && user.FullName.Length > 100)
        {
            ModelState.AddModelError("FullName", "Họ và tên không được dài quá 100 ký tự.");
            isValid = false;
        }

        if (isValid)
        {
            try
            {
                // Kiểm tra xem UserName đã tồn tại chưa
                if (_userRepository.GetAll().Any(u => u.UserName == user.UserName))
                {
                    ModelState.AddModelError("UserName", "Tên đăng nhập đã tồn tại.");
                    ViewBag.Roles = _roleRepository.GetAll().Where(r => r.RoleName != "Instructor");
                    return View(user);
                }

                // Kiểm tra RoleId có tồn tại không và không phải là Instructor
                var role = _roleRepository.GetById(user.RoleId);
                if (role == null || role.RoleName == "Instructor")
                {
                    ModelState.AddModelError("RoleId", "Vai trò không hợp lệ.");
                    ViewBag.Roles = _roleRepository.GetAll().Where(r => r.RoleName != "Instructor");
                    return View(user);
                }

                user.HashPassword(_passwordHasher, password);
                _userRepository.Add(user);
                _userRepository.Save();
                TempData["Success"] = "Thêm người dùng thành công!";
                return RedirectToAction("ManageUsers");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Đã có lỗi xảy ra: {ex.Message}";
            }
        }
        else
        {
            // Hiển thị lỗi validation chi tiết
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            TempData["Error"] = "Dữ liệu không hợp lệ: " + string.Join(", ", errors);
        }

        try
        {
            ViewBag.Roles = _roleRepository.GetAll().Where(r => r.RoleName != "Instructor");
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Đã có lỗi xảy ra khi tải danh sách vai trò: {ex.Message}";
        }

        return View(user);
    }

    [HttpGet]
    public IActionResult AddCourse()
    {
        return View(new Course());
    }

    [HttpPost]
    public IActionResult AddCourse(Course course)
    {
        // Gán CourseId trước khi kiểm tra validation
        course.CourseId = Guid.NewGuid().ToString();
        course.CreatedDate = DateTime.Now;

        // Kiểm tra các trường cần thiết
        bool isValid = true;

        if (string.IsNullOrEmpty(course.CourseName))
        {
            ModelState.AddModelError("CourseName", "Tên khóa học không được để trống.");
            isValid = false;
        }
        else if (course.CourseName.Length > 100)
        {
            ModelState.AddModelError("CourseName", "Tên khóa học không được dài quá 100 ký tự.");
            isValid = false;
        }

        if (string.IsNullOrEmpty(course.Description))
        {
            ModelState.AddModelError("Description", "Mô tả không được để trống.");
            isValid = false;
        }

        if (isValid)
        {
            try
            {
                _courseRepository.Add(course);
                _courseRepository.Save();
                TempData["Success"] = "Thêm khóa học thành công!";
                return RedirectToAction("ManageCourses");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Đã có lỗi xảy ra: {ex.Message}";
            }
        }
        else
        {
            // Hiển thị lỗi validation chi tiết
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            TempData["Error"] = "Dữ liệu không hợp lệ: " + string.Join(", ", errors);
        }

        return View(course);
    }

    [HttpGet]
    public IActionResult AddLesson()
    {
        try
        {
            var courses = _courseRepository.GetAll().ToList();
            if (!courses.Any())
            {
                TempData["Error"] = "Không có khóa học nào trong hệ thống. Vui lòng thêm khóa học trước.";
                return RedirectToAction("ManageLessons");
            }
            ViewBag.Courses = courses;
            return View(new Lesson());
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Đã có lỗi xảy ra: {ex.Message}";
            return RedirectToAction("ManageLessons");
        }
    }

    [HttpPost]
    public IActionResult AddLesson(Lesson lesson)
    {
        // Gán LessonId trước khi kiểm tra validation
        lesson.LessonId = Guid.NewGuid().ToString();

        // Kiểm tra các trường cần thiết
        bool isValid = true;

        if (string.IsNullOrEmpty(lesson.LessonTitle))
        {
            ModelState.AddModelError("LessonTitle", "Tiêu đề bài học không được để trống.");
            isValid = false;
        }
        else if (lesson.LessonTitle.Length > 100)
        {
            ModelState.AddModelError("LessonTitle", "Tiêu đề bài học không được dài quá 100 ký tự.");
            isValid = false;
        }

        if (string.IsNullOrEmpty(lesson.CourseId))
        {
            ModelState.AddModelError("CourseId", "Khóa học không được để trống.");
            isValid = false;
        }
        else if (lesson.CourseId.Length > 50)
        {
            ModelState.AddModelError("CourseId", "Khóa học không hợp lệ (quá dài).");
            isValid = false;
        }

        if (lesson.OrderNumber < 0)
        {
            ModelState.AddModelError("OrderNumber", "Số thứ tự không được nhỏ hơn 0.");
            isValid = false;
        }

        if (isValid)
        {
            try
            {
                // Kiểm tra CourseId có tồn tại không
                var course = _courseRepository.GetById(lesson.CourseId);
                if (course == null)
                {
                    ModelState.AddModelError("CourseId", "Khóa học không hợp lệ.");
                    ViewBag.Courses = _courseRepository.GetAll();
                    return View(lesson);
                }

                _lessonRepository.Add(lesson);
                _lessonRepository.Save();
                TempData["Success"] = "Thêm bài học thành công!";
                return RedirectToAction("ManageLessons");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Đã có lỗi xảy ra: {ex.Message}";
            }
        }
        else
        {
            // Hiển thị lỗi validation chi tiết
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            TempData["Error"] = "Dữ liệu không hợp lệ: " + string.Join(", ", errors);
        }

        try
        {
            ViewBag.Courses = _courseRepository.GetAll();
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Đã có lỗi xảy ra khi tải danh sách khóa học: {ex.Message}";
        }

        return View(lesson);
    }

    // Quản lý khóa học
    public IActionResult ManageCourses()
    {
        try
        {
            var courses = _courseRepository.GetAll();
            return View(courses);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Đã có lỗi xảy ra: {ex.Message}";
            return RedirectToAction("Dashboard");
        }
    }

    [HttpGet]
    public IActionResult EditCourse(string id)
    {
        try
        {
            var course = _courseRepository.GetById(id);
            if (course == null)
            {
                TempData["Error"] = "Không tìm thấy khóa học.";
                return RedirectToAction("ManageCourses");
            }
            return View(course);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Đã có lỗi xảy ra: {ex.Message}";
            return RedirectToAction("ManageCourses");
        }
    }

    [HttpPost]
    public IActionResult EditCourse(Course course)
    {
        // Kiểm tra các trường cần thiết
        bool isValid = true;

        if (string.IsNullOrEmpty(course.CourseId))
        {
            ModelState.AddModelError("CourseId", "ID khóa học không được để trống.");
            isValid = false;
        }

        if (string.IsNullOrEmpty(course.CourseName))
        {
            ModelState.AddModelError("CourseName", "Tên khóa học không được để trống.");
            isValid = false;
        }
        else if (course.CourseName.Length > 100)
        {
            ModelState.AddModelError("CourseName", "Tên khóa học không được dài quá 100 ký tự.");
            isValid = false;
        }

        if (string.IsNullOrEmpty(course.Description))
        {
            ModelState.AddModelError("Description", "Mô tả không được để trống.");
            isValid = false;
        }

        if (isValid)
        {
            try
            {
                var existingCourse = _courseRepository.GetById(course.CourseId);
                if (existingCourse == null)
                {
                    TempData["Error"] = "Không tìm thấy khóa học.";
                    return RedirectToAction("ManageCourses");
                }

                existingCourse.CourseName = course.CourseName;
                existingCourse.Description = course.Description;
                _courseRepository.Update(existingCourse);
                _courseRepository.Save();
                TempData["Success"] = "Cập nhật khóa học thành công!";
                return RedirectToAction("ManageCourses");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Đã có lỗi xảy ra: {ex.Message}";
            }
        }
        else
        {
            // Hiển thị lỗi validation chi tiết
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            TempData["Error"] = "Dữ liệu không hợp lệ: " + string.Join(", ", errors);
        }

        return View(course);
    }

    [HttpPost]
    public IActionResult DeleteCourse(string id)
    {
        try
        {
            var course = _courseRepository.GetById(id);
            if (course == null)
            {
                TempData["Error"] = "Không tìm thấy khóa học.";
                return RedirectToAction("ManageCourses");
            }

            _courseRepository.Delete(id);
            _courseRepository.Save();
            TempData["Success"] = "Xóa khóa học thành công!";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Đã có lỗi xảy ra: {ex.Message}";
        }
        return RedirectToAction("ManageCourses");
    }

    // Quản lý người dùng
    public IActionResult ManageUsers()
    {
        try
        {
            var users = _userRepository.GetAll();
            return View(users);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Đã có lỗi xảy ra: {ex.Message}";
            return RedirectToAction("Dashboard");
        }
    }

    [HttpGet]
    public IActionResult EditUser(string username)
    {
        try
        {
            var user = _userRepository.GetAll().FirstOrDefault(u => u.UserName == username);
            if (user == null)
            {
                TempData["Error"] = "Không tìm thấy người dùng.";
                return RedirectToAction("ManageUsers");
            }

            ViewBag.Roles = _roleRepository.GetAll().Where(r => r.RoleName != "Instructor");
            return View(user);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Đã có lỗi xảy ra: {ex.Message}";
            return RedirectToAction("ManageUsers");
        }
    }

    [HttpPost]
    public IActionResult EditUser(User user, string newPassword)
    {
        // Kiểm tra các trường cần thiết
        bool isValid = true;

        if (string.IsNullOrEmpty(user.UserName))
        {
            ModelState.AddModelError("UserName", "Tên đăng nhập không được để trống.");
            isValid = false;
        }
        else if (user.UserName.Length > 50)
        {
            ModelState.AddModelError("UserName", "Tên đăng nhập không được dài quá 50 ký tự.");
            isValid = false;
        }

        if (string.IsNullOrEmpty(user.Email))
        {
            ModelState.AddModelError("Email", "Email không được để trống.");
            isValid = false;
        }
        else if (user.Email.Length > 100)
        {
            ModelState.AddModelError("Email", "Email không được dài quá 100 ký tự.");
            isValid = false;
        }
        else if (!new EmailAddressAttribute().IsValid(user.Email))
        {
            ModelState.AddModelError("Email", "Email không hợp lệ.");
            isValid = false;
        }

        if (string.IsNullOrEmpty(user.RoleId))
        {
            ModelState.AddModelError("RoleId", "Vai trò không được để trống.");
            isValid = false;
        }
        else if (user.RoleId.Length > 50)
        {
            ModelState.AddModelError("RoleId", "Vai trò không hợp lệ (quá dài).");
            isValid = false;
        }

        if (user.FullName != null && user.FullName.Length > 100)
        {
            ModelState.AddModelError("FullName", "Họ và tên không được dài quá 100 ký tự.");
            isValid = false;
        }

        if (!string.IsNullOrEmpty(newPassword) && newPassword.Length > 100)
        {
            ModelState.AddModelError("newPassword", "Mật khẩu mới không được dài quá 100 ký tự.");
            isValid = false;
        }

        if (isValid)
        {
            try
            {
                var existingUser = _userRepository.GetAll().FirstOrDefault(u => u.UserName == user.UserName);
                if (existingUser == null)
                {
                    TempData["Error"] = "Không tìm thấy người dùng.";
                    return RedirectToAction("ManageUsers");
                }

                // Kiểm tra RoleId có tồn tại không và không phải là Instructor
                var role = _roleRepository.GetById(user.RoleId);
                if (role == null || role.RoleName == "Instructor")
                {
                    ModelState.AddModelError("RoleId", "Vai trò không hợp lệ.");
                    ViewBag.Roles = _roleRepository.GetAll().Where(r => r.RoleName != "Instructor");
                    return View(user);
                }

                existingUser.FullName = user.FullName;
                existingUser.Email = user.Email;
                existingUser.RoleId = user.RoleId;

                // Nếu người dùng nhập mật khẩu mới, cập nhật mật khẩu
                if (!string.IsNullOrEmpty(newPassword))
                {
                    existingUser.HashPassword(_passwordHasher, newPassword);
                }

                _userRepository.Update(existingUser);
                _userRepository.Save();
                TempData["Success"] = "Cập nhật người dùng thành công!";
                return RedirectToAction("ManageUsers");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Đã có lỗi xảy ra: {ex.Message}";
            }
        }
        else
        {
            // Hiển thị lỗi validation chi tiết
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            TempData["Error"] = "Dữ liệu không hợp lệ: " + string.Join(", ", errors);
        }

        try
        {
            ViewBag.Roles = _roleRepository.GetAll().Where(r => r.RoleName != "Instructor");
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Đã có lỗi xảy ra khi tải danh sách vai trò: {ex.Message}";
        }

        return View(user);
    }

    [HttpPost]
    public IActionResult DeleteUser(string username)
    {
        try
        {
            var user = _userRepository.GetAll().FirstOrDefault(u => u.UserName == username);
            if (user == null)
            {
                TempData["Error"] = "Không tìm thấy người dùng.";
                return RedirectToAction("ManageUsers");
            }

            _userRepository.Delete(user.UserName);
            _userRepository.Save();
            TempData["Success"] = "Xóa người dùng thành công!";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Đã có lỗi xảy ra: {ex.Message}";
        }
        return RedirectToAction("ManageUsers");
    }

    // Quản lý bài học
    public IActionResult ManageLessons()
    {
        try
        {
            var lessons = _lessonRepository.GetAll();
            return View(lessons);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Đã có lỗi xảy ra: {ex.Message}";
            return RedirectToAction("Dashboard");
        }
    }

    [HttpGet]
    public IActionResult EditLesson(string id)
    {
        try
        {
            var lesson = _lessonRepository.GetById(id);
            if (lesson == null)
            {
                TempData["Error"] = "Không tìm thấy bài học.";
                return RedirectToAction("ManageLessons");
            }

            ViewBag.Courses = _courseRepository.GetAll();
            return View(lesson);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Đã có lỗi xảy ra: {ex.Message}";
            return RedirectToAction("ManageLessons");
        }
    }

    [HttpPost]
    public IActionResult EditLesson(Lesson lesson)
    {
        // Kiểm tra các trường cần thiết
        bool isValid = true;

        if (string.IsNullOrEmpty(lesson.LessonId))
        {
            ModelState.AddModelError("LessonId", "ID bài học không được để trống.");
            isValid = false;
        }

        if (string.IsNullOrEmpty(lesson.LessonTitle))
        {
            ModelState.AddModelError("LessonTitle", "Tiêu đề bài học không được để trống.");
            isValid = false;
        }
        else if (lesson.LessonTitle.Length > 100)
        {
            ModelState.AddModelError("LessonTitle", "Tiêu đề bài học không được dài quá 100 ký tự.");
            isValid = false;
        }

        if (string.IsNullOrEmpty(lesson.CourseId))
        {
            ModelState.AddModelError("CourseId", "Khóa học không được để trống.");
            isValid = false;
        }
        else if (lesson.CourseId.Length > 50)
        {
            ModelState.AddModelError("CourseId", "Khóa học không hợp lệ (quá dài).");
            isValid = false;
        }

        if (lesson.OrderNumber < 0)
        {
            ModelState.AddModelError("OrderNumber", "Số thứ tự không được nhỏ hơn 0.");
            isValid = false;
        }

        if (isValid)
        {
            try
            {
                var existingLesson = _lessonRepository.GetById(lesson.LessonId);
                if (existingLesson == null)
                {
                    TempData["Error"] = "Không tìm thấy bài học.";
                    return RedirectToAction("ManageLessons");
                }

                // Kiểm tra CourseId có tồn tại không
                var course = _courseRepository.GetById(lesson.CourseId);
                if (course == null)
                {
                    ModelState.AddModelError("CourseId", "Khóa học không hợp lệ.");
                    ViewBag.Courses = _courseRepository.GetAll();
                    return View(lesson);
                }

                existingLesson.LessonTitle = lesson.LessonTitle;
                existingLesson.Content = lesson.Content;
                existingLesson.OrderNumber = lesson.OrderNumber;
                existingLesson.CourseId = lesson.CourseId;
                _lessonRepository.Update(existingLesson);
                _lessonRepository.Save();
                TempData["Success"] = "Cập nhật bài học thành công!";
                return RedirectToAction("ManageLessons");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Đã có lỗi xảy ra: {ex.Message}";
            }
        }
        else
        {
            // Hiển thị lỗi validation chi tiết
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            TempData["Error"] = "Dữ liệu không hợp lệ: " + string.Join(", ", errors);
        }

        try
        {
            ViewBag.Courses = _courseRepository.GetAll();
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Đã có lỗi xảy ra khi tải danh sách khóa học: {ex.Message}";
        }

        return View(lesson);
    }

    [HttpPost]
    public IActionResult DeleteLesson(string id)
    {
        try
        {
            var lesson = _lessonRepository.GetById(id);
            if (lesson == null)
            {
                TempData["Error"] = "Không tìm thấy bài học.";
                return RedirectToAction("ManageLessons");
            }

            _lessonRepository.Delete(id);
            _lessonRepository.Save();
            TempData["Success"] = "Xóa bài học thành công!";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Đã có lỗi xảy ra: {ex.Message}";
        }
        return RedirectToAction("ManageLessons");
    }

    // Quản lý bình luận
    public IActionResult ManageComments()
    {
        try
        {
            var comments = _commentRepository.GetAll();
            return View(comments);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Đã có lỗi xảy ra: {ex.Message}";
            return RedirectToAction("Dashboard");
        }
    }

    [HttpGet]
    public IActionResult AddComment()
    {
        try
        {
            ViewBag.Users = _userRepository.GetAll();
            ViewBag.Courses = _courseRepository.GetAll();
            return View(new Comment());
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Đã có lỗi xảy ra: {ex.Message}";
            return RedirectToAction("ManageComments");
        }
    }

    [HttpPost]
    public IActionResult AddComment(Comment comment)
    {
        // Gán CommentId và CreatedDate trước khi kiểm tra validation
        comment.CommentId = Guid.NewGuid().ToString();
        comment.CreatedDate = DateTime.Now;

        // Kiểm tra các trường cần thiết
        bool isValid = true;

        if (string.IsNullOrEmpty(comment.UserName))
        {
            ModelState.AddModelError("UserName", "Người dùng không được để trống.");
            isValid = false;
        }

        if (string.IsNullOrEmpty(comment.CourseId))
        {
            ModelState.AddModelError("CourseId", "Khóa học không được để trống.");
            isValid = false;
        }

        if (string.IsNullOrEmpty(comment.Content))
        {
            ModelState.AddModelError("Content", "Nội dung bình luận không được để trống.");
            isValid = false;
        }
        else if (comment.Content.Length > 1000)
        {
            ModelState.AddModelError("Content", "Nội dung bình luận không được dài quá 1000 ký tự.");
            isValid = false;
        }

        if (isValid)
        {
            try
            {
                // Kiểm tra UserName có tồn tại không
                var user = _userRepository.GetAll().FirstOrDefault(u => u.UserName == comment.UserName);
                if (user == null)
                {
                    ModelState.AddModelError("UserName", "Người dùng không hợp lệ.");
                    ViewBag.Users = _userRepository.GetAll();
                    ViewBag.Courses = _courseRepository.GetAll();
                    return View(comment);
                }

                // Kiểm tra CourseId có tồn tại không
                var course = _courseRepository.GetById(comment.CourseId);
                if (course == null)
                {
                    ModelState.AddModelError("CourseId", "Khóa học không hợp lệ.");
                    ViewBag.Users = _userRepository.GetAll();
                    ViewBag.Courses = _courseRepository.GetAll();
                    return View(comment);
                }

                _commentRepository.Add(comment);
                _commentRepository.Save();
                TempData["Success"] = "Thêm bình luận thành công!";
                return RedirectToAction("ManageComments");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Đã có lỗi xảy ra: {ex.Message}";
            }
        }
        else
        {
            // Hiển thị lỗi validation chi tiết
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            TempData["Error"] = "Dữ liệu không hợp lệ: " + string.Join(", ", errors);
        }

        try
        {
            ViewBag.Users = _userRepository.GetAll();
            ViewBag.Courses = _courseRepository.GetAll();
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Đã có lỗi xảy ra khi tải dữ liệu: {ex.Message}";
        }

        return View(comment);
    }

    [HttpGet]
    public IActionResult EditComment(string id)
    {
        try
        {
            var comment = _commentRepository.GetById(id);
            if (comment == null)
            {
                TempData["Error"] = "Không tìm thấy bình luận.";
                return RedirectToAction("ManageComments");
            }

            ViewBag.Users = _userRepository.GetAll();
            ViewBag.Courses = _courseRepository.GetAll();
            return View(comment);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Đã có lỗi xảy ra: {ex.Message}";
            return RedirectToAction("ManageComments");
        }
    }

    [HttpPost]
    public IActionResult EditComment(Comment comment)
    {
        // Kiểm tra các trường cần thiết
        bool isValid = true;

        if (string.IsNullOrEmpty(comment.CommentId))
        {
            ModelState.AddModelError("CommentId", "ID bình luận không được để trống.");
            isValid = false;
        }

        if (string.IsNullOrEmpty(comment.UserName))
        {
            ModelState.AddModelError("UserName", "Người dùng không được để trống.");
            isValid = false;
        }

        if (string.IsNullOrEmpty(comment.CourseId))
        {
            ModelState.AddModelError("CourseId", "Khóa học không được để trống.");
            isValid = false;
        }

        if (string.IsNullOrEmpty(comment.Content))
        {
            ModelState.AddModelError("Content", "Nội dung bình luận không được để trống.");
            isValid = false;
        }
        else if (comment.Content.Length > 1000)
        {
            ModelState.AddModelError("Content", "Nội dung bình luận không được dài quá 1000 ký tự.");
            isValid = false;
        }

        if (isValid)
        {
            try
            {
                var existingComment = _commentRepository.GetById(comment.CommentId);
                if (existingComment == null)
                {
                    TempData["Error"] = "Không tìm thấy bình luận.";
                    return RedirectToAction("ManageComments");
                }

                // Kiểm tra UserName có tồn tại không
                var user = _userRepository.GetAll().FirstOrDefault(u => u.UserName == comment.UserName);
                if (user == null)
                {
                    ModelState.AddModelError("UserName", "Người dùng không hợp lệ.");
                    ViewBag.Users = _userRepository.GetAll();
                    ViewBag.Courses = _courseRepository.GetAll();
                    return View(comment);
                }

                // Kiểm tra CourseId có tồn tại không
                var course = _courseRepository.GetById(comment.CourseId);
                if (course == null)
                {
                    ModelState.AddModelError("CourseId", "Khóa học không hợp lệ.");
                    ViewBag.Users = _userRepository.GetAll();
                    ViewBag.Courses = _courseRepository.GetAll();
                    return View(comment);
                }

                existingComment.UserName = comment.UserName;
                existingComment.CourseId = comment.CourseId;
                existingComment.Content = comment.Content;
                existingComment.CreatedDate = comment.CreatedDate;
                _commentRepository.Update(existingComment);
                _commentRepository.Save();
                TempData["Success"] = "Cập nhật bình luận thành công!";
                return RedirectToAction("ManageComments");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Đã có lỗi xảy ra: {ex.Message}";
            }
        }
        else
        {
            // Hiển thị lỗi validation chi tiết
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            TempData["Error"] = "Dữ liệu không hợp lệ: " + string.Join(", ", errors);
        }

        try
        {
            ViewBag.Users = _userRepository.GetAll();
            ViewBag.Courses = _courseRepository.GetAll();
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Đã có lỗi xảy ra khi tải dữ liệu: {ex.Message}";
        }

        return View(comment);
    }

    [HttpPost]
    public IActionResult DeleteComment(string id)
    {
        try
        {
            var comment = _commentRepository.GetById(id);
            if (comment == null)
            {
                TempData["Error"] = "Không tìm thấy bình luận.";
                return RedirectToAction("ManageComments");
            }

            _commentRepository.Delete(id);
            _commentRepository.Save();
            TempData["Success"] = "Xóa bình luận thành công!";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Đã có lỗi xảy ra: {ex.Message}";
        }
        return RedirectToAction("ManageComments");
    }
}