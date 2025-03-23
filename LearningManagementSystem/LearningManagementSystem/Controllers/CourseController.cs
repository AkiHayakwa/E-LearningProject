using LearningManagementSystem.Models;
using LearningManagementSystem.Repositories;
using Microsoft.AspNetCore.Mvc;

public class CourseController : Controller
{
    private readonly ICourseRepository _courseRepository;

    public CourseController(ICourseRepository courseRepository)
    {
        _courseRepository = courseRepository;
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
}