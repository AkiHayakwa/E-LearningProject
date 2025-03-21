using LearningManagementSystem.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningManagementSystem.Controllers
{
    [Authorize]
    public class LessonController : Controller
    {
        private readonly ILessonRepository _lessonRepository;
        private readonly ICourseRepository _courseRepository;

        public LessonController(ILessonRepository lessonRepository, ICourseRepository courseRepository)
        {
            _lessonRepository = lessonRepository;
            _courseRepository = courseRepository;
        }

        public IActionResult Details(string lessonId)
        {
            var lesson = _lessonRepository.GetById(lessonId);
            if (lesson == null) return NotFound();

            var course = _courseRepository.GetById(lesson.CourseId);
            ViewBag.CourseName = course?.CourseName;
            return View(lesson);
        }
    }
}
