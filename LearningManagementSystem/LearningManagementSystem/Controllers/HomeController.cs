using LearningManagementSystem.Data;
using LearningManagementSystem.Models;
using LearningManagementSystem.Models.ViewModels;
using LearningManagementSystem.Repositories;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

public class HomeController : Controller
{
    private readonly IUserRepository _userRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly IProgressRepository _progressRepository;
    private readonly ICommentRepository _commentRepository;

    public HomeController(
        IUserRepository userRepository,
        ICourseRepository courseRepository,
        IEnrollmentRepository enrollmentRepository,
        IProgressRepository progressRepository,
        ICommentRepository commentRepository)
    {
        _userRepository = userRepository;
        _courseRepository = courseRepository;
        _enrollmentRepository = enrollmentRepository;
        _progressRepository = progressRepository;
        _commentRepository = commentRepository;
    }

    public IActionResult Index()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userId))
        {
            return RedirectToAction("Login", "Account");
        }

        var user = _userRepository.GetById(userId);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var model = new HomeViewModel
        {
            User = user,
            Enrollments = _enrollmentRepository.GetEnrollmentsByUser(userId)
                .OrderByDescending(e => e.EnrollmentDate)
                .Take(5)
                .ToList(),
            Progresses = _progressRepository.GetProgressByUser(userId)
                .OrderByDescending(p => p.CompletionDate)
                .Take(5)
                .ToList(),
            Comments = _commentRepository.GetCommentsByUser(userId)
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
}