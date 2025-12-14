using System;
using Microsoft.AspNetCore.Mvc;
using LearningManagementSystem.Data;
using LearningManagementSystem.Models;
using LearningManagementSystem.Models.ViewModels;
using LearningManagementSystem.Repositories;
using System.Linq;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

public class SearchController : Controller
{
    private readonly LMSContext _context;
    private readonly IEnrollmentRepository _enrollmentRepository;

    public SearchController(LMSContext context, IEnrollmentRepository enrollmentRepository)
    {
        _context = context;
        _enrollmentRepository = enrollmentRepository;
    }

    public IActionResult Index(string query, string sort = "name-asc", string[] tagIds = null, string[] levels = null, int? minDuration = null, int? maxDuration = null)
    {
        // Sử dụng User.Identity.Name để lấy username của người dùng đã đăng nhập
        var userName = User.Identity.IsAuthenticated ? User.Identity.Name : null;

        var normalizedTagIds = tagIds?.Where(t => !string.IsNullOrWhiteSpace(t)).Select(t => t.Trim()).ToArray() ?? Array.Empty<string>();
        var normalizedLevels = levels?.Where(l => !string.IsNullOrWhiteSpace(l)).Select(l => l.Trim()).ToArray() ?? Array.Empty<string>();

        var baseQuery = _context.Courses
            .Include(c => c.Lessons)
            .Include(c => c.Comments)
            .Include(c => c.CourseTags)
                .ThenInclude(ct => ct.Tag)
            .Include(c => c.CourseInstructors)
                .ThenInclude(ci => ci.User)
                    .ThenInclude(u => u.Role)
            .Where(c => string.IsNullOrEmpty(query) || c.CourseName.Contains(query));

        if (normalizedLevels.Any())
        {
            baseQuery = baseQuery.Where(c => c.Level != null && normalizedLevels.Contains(c.Level));
        }

        if (normalizedTagIds.Any())
        {
            baseQuery = baseQuery.Where(c => c.CourseTags.Any(ct => normalizedTagIds.Contains(ct.TagId)));
        }

        if (minDuration.HasValue)
        {
            baseQuery = baseQuery.Where(c => c.DurationMinutes >= minDuration.Value);
        }

        if (maxDuration.HasValue)
        {
            baseQuery = baseQuery.Where(c => !c.DurationMinutes.HasValue || c.DurationMinutes <= maxDuration.Value);
        }

        // Materialize courses trước để có thể map InstructorName
        var coursesList = baseQuery.ToList();

        // Map sang CourseListViewModel với InstructorName
        var coursesQuery = coursesList.Select(c =>
        {
            // Lấy tên giảng viên
            string instructorName = "Chưa có giảng viên";
            if (c.CourseInstructors?.Any() == true)
            {
                var instructor = c.CourseInstructors
                    .FirstOrDefault(ci => ci.User?.Role?.RoleName == "Instructor")?.User;
                if (instructor != null)
                {
                    instructorName = instructor.FullName ?? instructor.UserName ?? "Chưa có giảng viên";
                }
            }

            return new CourseListViewModel
            {
                CourseId = c.CourseId,
                CourseName = c.CourseName,
                Title = c.CourseName,
                Description = c.Description,
                CreatedDate = c.CreatedDate,
                ImageUrl = c.ImageUrl,
                Lessons = c.Lessons.ToList(),
                IsEnrolled = userName != null && _enrollmentRepository.GetAll()
                    .Any(e => e.UserName == userName && e.CourseId == c.CourseId),
                Price = c.Price,
                AverageRating = c.Comments.Any() ? c.Comments.Average(cm => cm.Rating) : null,
                Level = c.Level,
                DurationMinutes = c.DurationMinutes,
                InstructorName = instructorName,
                TagNames = c.CourseTags
                    .Select(ct => ct.Tag != null ? ct.Tag.Name : string.Empty)
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .ToList()
            };
        });

        // Sắp xếp danh sách khóa học dựa trên tham số sort
        List<CourseListViewModel> courses;
        switch (sort)
        {
            case "rating-desc": // Sắp xếp theo đánh giá cao (giảm dần)
                courses = coursesQuery
                    .OrderByDescending(c => c.AverageRating ?? 0) // Sắp xếp giảm dần, ưu tiên các khóa học có điểm cao, nếu null thì coi là 0
                    .ToList();
                break;
            case "name-asc": // Sắp xếp theo tên A-Z (tăng dần)
            default:
                courses = coursesQuery
                    .OrderBy(c => c.CourseName) // Sắp xếp tăng dần theo tên
                    .ToList();
                break;
        }

        ViewBag.Query = query;
        ViewBag.Sort = sort; // Truyền giá trị sort để view biết tiêu chí hiện tại
        ViewBag.AvailableTags = _context.Tags
            .Where(t => t.IsActive)
            .OrderBy(t => t.Category)
            .ThenBy(t => t.Name)
            .ToList();
        ViewBag.SelectedTags = normalizedTagIds;
        ViewBag.SelectedLevels = normalizedLevels;
        ViewBag.MinDuration = minDuration;
        ViewBag.MaxDuration = maxDuration;
        return View(courses);
    }

    [HttpGet]
    public async Task<IActionResult> GetSearchSuggestions(string query)
    {
        if (string.IsNullOrEmpty(query))
        {
            return Json(new List<string>());
        }

        var suggestions = await _context.Courses
            .Where(c => c.CourseName.Contains(query))
            .Select(c => c.CourseName)
            .Take(5) // Giới hạn 5 gợi ý
            .ToListAsync();

        return Json(suggestions);
    }
}