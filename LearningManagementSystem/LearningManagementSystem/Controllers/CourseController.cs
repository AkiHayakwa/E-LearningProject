using LearningManagementSystem.Models.ViewModels;
using LearningManagementSystem.Models;
using LearningManagementSystem.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
}