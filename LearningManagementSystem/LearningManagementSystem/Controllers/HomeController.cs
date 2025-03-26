using LearningManagementSystem.Models;
using LearningManagementSystem.Models.ViewModels;
using LearningManagementSystem.Repositories;
<<<<<<< Updated upstream
=======
using Microsoft.AspNetCore.Authorization;
>>>>>>> Stashed changes
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

[Authorize(Roles = "Student")]
public class HomeController : Controller
{
    private readonly IUserRepository _userRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly IProgressRepository _progressRepository;
    private readonly ICommentRepository _commentRepository;
    private readonly ILogger<HomeController> _logger;

    public HomeController(
        IUserRepository userRepository,
        ICourseRepository courseRepository,
        IEnrollmentRepository enrollmentRepository,
        IProgressRepository progressRepository,
        ICommentRepository commentRepository,
        ILogger<HomeController> logger)
    {
        _userRepository = userRepository;
        _courseRepository = courseRepository;
        _enrollmentRepository = enrollmentRepository;
        _progressRepository = progressRepository;
        _commentRepository = commentRepository;
        _logger = logger;
    }

    public IActionResult Index()
    {
        _logger.LogInformation("Index called.");

        var userName = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userName))
        {
            _logger.LogWarning("UserName could not be determined from claims.");
            return RedirectToAction("Login", "Account");
        }

        var user = _userRepository.GetById(userName);
        if (user == null)
        {
            _logger.LogWarning($"User with UserName: {userName} not found.");
            return RedirectToAction("Login", "Account");
        }

        var model = new HomeViewModel
        {
            User = user,
            Enrollments = _enrollmentRepository.GetAll()
                .Where(e => e.UserName == userName)
                .OrderByDescending(e => e.EnrollmentDate)
                .Take(5)
                .ToList(),
            Progresses = _progressRepository.GetAll()
                .Where(p => p.UserName == userName)
                .OrderByDescending(p => p.CompletionDate)
                .Take(5)
                .ToList(),
            Comments = _commentRepository.GetAll()
                .Where(c => c.UserName == userName)
                .OrderByDescending(c => c.CreatedDate)
                .Take(5)
                .ToList()
        };

        var enrolledCourseIds = model.Enrollments.Select(e => e.CourseId).ToList();
        var availableCourses = _courseRepository.GetAll()
            .Where(c => !enrolledCourseIds.Contains(c.CourseId))
            .OrderBy(c => c.CreatedDate)
            .ToList();

        ViewBag.AvailableCourses = availableCourses;
        return View(model);
    }
<<<<<<< Updated upstream
=======

    public IActionResult Courses()
    {
        _logger.LogInformation("Courses called.");

        var userName = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userName))
        {
            _logger.LogWarning("UserName could not be determined from claims.");
            return RedirectToAction("Login", "Account");
        }

        var enrolledCourseIds = _enrollmentRepository.GetAll()
            .Where(e => e.UserName == userName)
            .Select(e => e.CourseId)
            .ToList();

        var courses = _courseRepository.GetAll()
            .Where(c => !enrolledCourseIds.Contains(c.CourseId))
            .OrderBy(c => c.CreatedDate)
            .ToList();

        return View(courses);
    }
>>>>>>> Stashed changes
}