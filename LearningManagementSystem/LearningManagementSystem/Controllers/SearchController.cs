using Microsoft.AspNetCore.Mvc;
using LearningManagementSystem.Data;
using LearningManagementSystem.Models;
using System.Linq;

public class SearchController : Controller
{
    private readonly LMSContext _context;

    public SearchController(LMSContext context)
    {
        _context = context;
    }

    public IActionResult Index(string query)
    {
        var courses = _context.Courses
            .Where(c => string.IsNullOrEmpty(query) || c.CourseName.Contains(query))
            .ToList();

        ViewBag.Query = query;
        return View(courses);
    }
}