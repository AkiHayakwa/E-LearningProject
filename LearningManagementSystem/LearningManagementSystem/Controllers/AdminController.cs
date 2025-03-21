using LearningManagementSystem.Data;
using LearningManagementSystem.Models;
using LearningManagementSystem.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly LMSContext _context;

        public AdminController(IUserRepository userRepository, ICourseRepository courseRepository, ILessonRepository lessonRepository, LMSContext context)
        {
            _userRepository = userRepository;
            _courseRepository = courseRepository;
            _lessonRepository = lessonRepository;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        // Quản lý người dùng
        public IActionResult ManageUsers()
        {
            var users = _userRepository.GetAll();
            return View(users);
        }

        [HttpGet]
        public IActionResult CreateUser()
        {
            ViewBag.Roles = _context.Roles.ToList();
            return View();
        }

        [HttpPost]
        public IActionResult CreateUser(User user)
        {
            if (ModelState.IsValid)
            {
                user.UserId = Guid.NewGuid().ToString();
                _userRepository.Add(user);
                _context.SaveChanges();
                return RedirectToAction("ManageUsers");
            }
            ViewBag.Roles = _context.Roles.ToList();
            return View(user);
        }

        [HttpPost]
        public IActionResult DeleteUser(string id)
        {
            var user = _userRepository.GetById(id);
            if (user == null) return NotFound();
            _userRepository.Delete(user);
            _context.SaveChanges();
            return RedirectToAction("ManageUsers");
        }

        // Quản lý khóa học
        public IActionResult ManageCourses()
        {
            var courses = _courseRepository.GetAll();
            return View(courses);
        }

        [HttpGet]
        public IActionResult CreateCourse()
        {
            ViewBag.Instructors = _userRepository.GetAll().Where(u => u.Roles.RoleName == "Instructor");
            return View();
        }

        [HttpPost]
        public IActionResult CreateCourse(Course course)
        {
            if (ModelState.IsValid)
            {
                course.CourseId = Guid.NewGuid().ToString();
                _courseRepository.Add(course);
                _context.SaveChanges();
                return RedirectToAction("ManageCourses");
            }
            ViewBag.Instructors = _userRepository.GetAll().Where(u => u.Roles.RoleName == "Instructor");
            return View(course);
        }

        [HttpPost]
        public IActionResult DeleteCourse(string id)
        {
            var course = _courseRepository.GetById(id);
            if (course == null) return NotFound();
            _courseRepository.Delete(course);
            _context.SaveChanges();
            return RedirectToAction("ManageCourses");
        }

        // Quản lý bài học
        public IActionResult ManageLessons(string courseId)
        {
            var lessons = _lessonRepository.GetLessonsByCourseId(courseId);
            ViewBag.CourseId = courseId;
            return View(lessons);
        }

        [HttpGet]
        public IActionResult CreateLesson(string courseId)
        {
            ViewBag.CourseId = courseId;
            return View();
        }

        [HttpPost]
        public IActionResult CreateLesson(Lesson lesson)
        {
            if (ModelState.IsValid)
            {
                lesson.LessonId = Guid.NewGuid().ToString();
                _lessonRepository.Add(lesson);
                _context.SaveChanges();
                return RedirectToAction("ManageLessons", new { courseId = lesson.CourseId });
            }
            ViewBag.CourseId = lesson.CourseId;
            return View(lesson);
        }

        [HttpPost]
        public IActionResult DeleteLesson(string id, string courseId)
        {
            var lesson = _lessonRepository.GetById(id);
            if (lesson == null) return NotFound();
            _lessonRepository.Delete(lesson);
            _context.SaveChanges();
            return RedirectToAction("ManageLessons", new { courseId });
        }
    }
}
