using LearningManagementSystem.Models;
using LearningManagementSystem.Models.ViewModels;
using LearningManagementSystem.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using LearningManagementSystem.Data;

namespace LearningManagementSystem.Controllers
{
    public class CourseController : Controller
    {
        private readonly ICourseRepository _courseRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly IProgressRepository _progressRepository; // Thêm repository cho Progress
        private readonly IAssignmentRepository _assignmentRepository;
        private readonly IAssignmentQuestionRepository _assignmentQuestionRepository;
        private readonly IAssignmentQuestionOptionRepository _assignmentQuestionOptionRepository;
        private readonly ILogger<CourseController> _logger;
        private readonly IUserRepository _userRepository;
        private readonly LMSContext _context;

        public CourseController(
            ICourseRepository courseRepository,
            ICommentRepository commentRepository,
            IEnrollmentRepository enrollmentRepository,
            ILessonRepository lessonRepository,
            IProgressRepository progressRepository,
            IUserRepository userRepository,
            IAssignmentRepository assignmentRepository,
            IAssignmentQuestionRepository assignmentQuestionRepository,
            IAssignmentQuestionOptionRepository assignmentQuestionOptionRepository,
            LMSContext context,
            ILogger<CourseController> logger)
        {
            _courseRepository = courseRepository;
            _commentRepository = commentRepository;
            _enrollmentRepository = enrollmentRepository;
            _lessonRepository = lessonRepository;
            _progressRepository = progressRepository;
            _assignmentRepository = assignmentRepository;
            _userRepository = userRepository;
            _assignmentQuestionRepository = assignmentQuestionRepository;
            _assignmentQuestionOptionRepository = assignmentQuestionOptionRepository;
            _logger = logger;
            _context = context;
        }

        // GET: Course/Index
        public IActionResult Index()
        {
            _logger.LogInformation("Index called to display list of courses.");

            var userName = User.Identity.IsAuthenticated ? User.FindFirst(ClaimTypes.Name)?.Value : null;
            var courses = _courseRepository.GetAll()
                .Select(c => new CourseListViewModel
                {
                    CourseId = c.CourseId,
                    CourseName = c.CourseName,
                    Description = c.Description,
                    CreatedDate = c.CreatedDate,
                    ImageUrl = c.ImageUrl,
                    IsEnrolled = userName != null && _enrollmentRepository.GetAll()
                        .Any(e => e.UserName == userName && e.CourseId == c.CourseId),
                    Level = c.Level,
                    DurationMinutes = c.DurationMinutes,
                    TagNames = c.CourseTags != null
                        ? c.CourseTags
                            .Select(ct => ct.Tag != null ? ct.Tag.Name : string.Empty)
                            .Where(name => !string.IsNullOrWhiteSpace(name))
                            .ToList()
                        : new List<string>()
                })
                .ToList();

            return View(courses);
        }
        
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Details(string id, int page = 1)
        {
            if (string.IsNullOrEmpty(id))
            {
                _logger.LogWarning("Course ID is null or empty.");
                return BadRequest("ID khóa học không hợp lệ.");
            }

            var course = await _context.Courses
                .Include(c => c.Lessons)
                    .ThenInclude(l => l.Assignments)
                        .ThenInclude(a => a.Questions)
                            .ThenInclude(q => q.Options)
                .Include(c => c.Lessons)
                .Include(c => c.CourseInstructors)
                    .ThenInclude(ci => ci.User)
                .Include(c => c.CourseTags)
                    .ThenInclude(ct => ct.Tag)
                .FirstOrDefaultAsync(c => c.CourseId == id);

            if (course == null)
            {
                _logger.LogWarning($"Course with ID {id} not found.");
                return NotFound("Không tìm thấy khóa học.");
            }

            var userName = User.Identity.Name;
            if (string.IsNullOrEmpty(userName))
            {
                _logger.LogWarning("UserName could not be determined from claims.");
                return Unauthorized("Bạn cần đăng nhập để xem chi tiết khóa học.");
            }

            var isEnrolled = await _context.Enrollments
                .AnyAsync(e => e.UserName == userName && e.CourseId == id);

            var progresses = await _context.Progresses
                .Include(p => p.Lesson)
                .Where(p => p.UserName == userName && p.Lesson.CourseId == id)
                .ToListAsync();

            // Phân trang cho comments
            int pageSize = 5;
            page = page < 1 ? 1 : page;

            var allCommentsQuery = _context.Comments
                .Where(c => c.CourseId == id)
                .Include(c => c.User);

            // Calculate AverageRating from ALL comments (not just current page)
            var allCommentsForRating = await allCommentsQuery.ToListAsync();
            double? averageRating = allCommentsForRating.Any() && allCommentsForRating.Any(c => c.Rating.HasValue)
                ? allCommentsForRating.Where(c => c.Rating.HasValue).Average(c => c.Rating.Value)
                : (double?)null;

            var totalComments = allCommentsForRating.Count;
            var totalPages = (int)Math.Ceiling(totalComments / (double)pageSize);

            var comments = await allCommentsQuery
                .OrderByDescending(c => c.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Khởi tạo dictionary cho bài nộp trước đó
            Dictionary<string, (string SelectedOptionText, string SelectedOptionLabel, bool? IsCorrect, double? Score)> previousSubmissions = new Dictionary<string, (string, string, bool?, double?)>();

            if (isEnrolled)
            {
                var questionIds = course.Lessons?
                    .SelectMany(l => l.Assignments ?? new List<Assignment>())
                    .SelectMany(a => a.Questions ?? new List<AssignmentQuestion>())
                    .Select(q => q.QuestionId)
                    .ToList() ?? new List<string>();

                if (questionIds.Any())
                {
                    var submissions = await _context.AssignmentSubmissions
                        .Where(s => s.UserName == userName && questionIds.Contains(s.QuestionId))
                        .ToListAsync();

                    if (submissions != null && submissions.Any())
                    {
                        previousSubmissions = submissions
                            .GroupBy(s => s.QuestionId)
                            .ToDictionary(
                                g => g.Key,
                                g =>
                                {
                                    var submission = g.FirstOrDefault();
                                    if (submission != null)
                                    {
                                        return (
                                            SelectedOptionText: submission.SelectedOptionText ?? "",
                                            SelectedOptionLabel: submission.SelectedOptionLabel ?? "",
                                            IsCorrect: submission.IsCorrect,
                                            Score: submission.Score
                                        );
                                    }
                                    return (SelectedOptionText: "", SelectedOptionLabel: "", IsCorrect: null, Score: null);
                                });
                    }
                }
            }

            _logger.LogInformation($"Course ID: {id}, Title: {course.CourseName}, Lessons Count: {(course.Lessons?.Count ?? 0)}");
            foreach (var lesson in course.Lessons ?? new List<Lesson>())
            {
                _logger.LogInformation($"Lesson ID: {lesson.LessonId}, Title: {lesson.LessonTitle}, Assignments Count: {(lesson.Assignments?.Count ?? 0)}");
                if (lesson.Assignments != null && lesson.Assignments.Any())
                {
                    foreach (var assignment in lesson.Assignments)
                    {
                        _logger.LogInformation($"Assignment ID: {assignment.AssignmentId}, Title: {assignment.Title}, Questions Count: {(assignment.Questions?.Count ?? 0)}");
                    }
                }
            }
            _logger.LogInformation($"Progresses Count: {(progresses?.Count ?? 0)}, IsEnrolled: {isEnrolled}, PreviousSubmissions Count: {previousSubmissions.Count}");

            var instructorName = course.CourseInstructors?.FirstOrDefault()?.User?.FullName ?? "Không xác định";

            var studyStats = new StudyTimerStatsViewModel
            {
                TotalMinutes = 0,
                WeeklyMinutes = 0,
                CurrentStreakDays = 0,
                TotalSessions = 0,
                LastStudiedAt = null
            };

            if (User.Identity.IsAuthenticated)
            {
                userName = User.FindFirstValue(ClaimTypes.Name);
                if (!string.IsNullOrWhiteSpace(userName))
                {
                    var studySessions = await _context.StudySessions
                        .Where(ss => ss.UserName == userName && ss.CourseId == course.CourseId)
                        .OrderByDescending(ss => ss.StartedAt)
                        .ToListAsync();

                    if (studySessions.Any())
                    {
                        var totalSeconds = studySessions.Sum(ss => ss.DurationSeconds);
                        var weekStart = DateTime.UtcNow.Date.AddDays(-6);
                        var weeklySeconds = studySessions
                            .Where(ss => ss.StartedAt.Date >= weekStart)
                            .Sum(ss => ss.DurationSeconds);

                        var lastSession = studySessions.FirstOrDefault();
                        var lastStudied = lastSession?.EndedAt ?? lastSession?.StartedAt;

                        studyStats.TotalMinutes = (int)Math.Round(totalSeconds / 60.0);
                        studyStats.WeeklyMinutes = (int)Math.Round(weeklySeconds / 60.0);
                        studyStats.CurrentStreakDays = CalculateStudyStreakDays(studySessions);
                        studyStats.TotalSessions = studySessions.Count;
                        studyStats.LastStudiedAt = lastStudied?.ToLocalTime();
                    }
                }
            }

            // Map Course to CourseListViewModel
            var courseListViewModel = new CourseListViewModel
            {
                CourseId = course.CourseId,
                CourseName = course.CourseName,
                Description = course.Description,
                CreatedDate = course.CreatedDate,
                ImageUrl = course.ImageUrl,
                IsEnrolled = isEnrolled,
                Lessons = course.Lessons,
                Assignments = course.Lessons?.SelectMany(l => l.Assignments ?? new List<Assignment>()).ToList(),
                AverageRating = averageRating,
                Price = course.Price,
                InstructorName = instructorName,
                Level = course.Level,
                DurationMinutes = course.DurationMinutes,
                TagNames = course.CourseTags
                    .Select(ct => ct.Tag != null ? ct.Tag.Name : string.Empty)
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .ToList()
            };

            var viewModel = new CourseDetailsViewModel
            {
                Course = courseListViewModel, // This will cause a type mismatch
                IsEnrolled = isEnrolled,
                Progresses = progresses ?? new List<Progress>(),
                Comments = comments ?? new List<Comment>(),
                PreviousSubmissions = previousSubmissions,
                NewCommentContent = "",
                NewCommentRating = 0,
                StudyStats = studyStats,
                CommentPage = page,
                CommentPageSize = pageSize,
                CommentTotalPages = totalPages,
                CommentTotalCount = totalComments
            };

            ViewBag.CommentPage = page;
            ViewBag.CommentTotalPages = totalPages;
            ViewBag.CommentTotalCount = totalComments;
            ViewBag.CourseId = id;

            return View("CourseDetails", viewModel);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(string id, CourseDetailsViewModel model)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(model.NewCommentContent))
            {
                TempData["Error"] = "Nội dung bình luận không được để trống.";
                return RedirectToAction("Details", new { id });
            }

            // Validate rating
            if (model.NewCommentRating < 1 || model.NewCommentRating > 5)
            {
                TempData["Error"] = "Đánh giá phải từ 1 đến 5 sao.";
                return RedirectToAction("Details", new { id });
            }

            var course = await _context.Courses
                .Include(c => c.Lessons)
                .FirstOrDefaultAsync(c => c.CourseId == id);

            if (course == null)
            {
                _logger.LogWarning($"Course with ID {id} not found.");
                return NotFound("Không tìm thấy khóa học.");
            }

            var userName = User.Identity.Name;
            var isEnrolled = await _context.Enrollments.AnyAsync(e => e.UserName == userName && e.CourseId == id);
            if (!isEnrolled)
            {
                TempData["Error"] = "Bạn cần đăng ký khóa học để bình luận.";
                return RedirectToAction("Details", new { id });
            }

            var comment = new Comment
            {
                CommentId = Guid.NewGuid().ToString(),
                UserName = userName,
                CourseId = id,
                Content = model.NewCommentContent,
                CreatedDate = DateTime.Now,
                Rating = model.NewCommentRating // Lưu giá trị Rating từ form
            };

            try
            {
                _context.Comments.Add(comment);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Comment added by {userName} for course {id} with rating {comment.Rating}.");
                TempData["Success"] = "Bình luận đã được thêm!";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to add comment by {userName} for course {id}: {ex.Message}");
                TempData["Error"] = "Lỗi khi thêm bình luận.";
            }

            return RedirectToAction("Details", new { id });
        }

        private static int CalculateStudyStreakDays(IEnumerable<StudySession> sessions)
        {
            var sessionDates = sessions
                .Where(ss => ss.DurationSeconds > 0)
                .Select(ss => ss.StartedAt.Date)
                .Distinct()
                .OrderByDescending(d => d)
                .ToList();

            if (!sessionDates.Any())
            {
                return 0;
            }

            var todayUtc = DateTime.UtcNow.Date;
            var startDate = sessionDates.First();
            if (sessionDates.Contains(todayUtc))
            {
                startDate = todayUtc;
            }

            var streak = 0;
            var currentDate = startDate;
            var dateSet = new HashSet<DateTime>(sessionDates);

            while (dateSet.Contains(currentDate))
            {
                streak++;
                currentDate = currentDate.AddDays(-1);
            }

            return streak;
        }
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditComment(string id, string commentId, string content, int rating)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(commentId) || string.IsNullOrEmpty(content))
            {
                TempData["Error"] = "Thông tin chỉnh sửa không hợp lệ.";
                return RedirectToAction("Details", new { id });
            }

            if (rating < 1 || rating > 5)
            {
                TempData["Error"] = "Đánh giá phải từ 1 đến 5 sao.";
                return RedirectToAction("Details", new { id });
            }

            var comment = await _context.Comments
                .FirstOrDefaultAsync(c => c.CommentId == commentId && c.CourseId == id);

            if (comment == null)
            {
                _logger.LogWarning($"Comment with ID {commentId} not found for course {id}.");
                return NotFound("Không tìm thấy bình luận.");
            }

            var userName = User.Identity.Name;
            if (comment.UserName != userName)
            {
                _logger.LogWarning($"User {userName} attempted to edit comment {commentId} that they do not own.");
                TempData["Error"] = "Bạn không có quyền chỉnh sửa bình luận này.";
                return RedirectToAction("Details", new { id });
            }

            try
            {
                comment.Content = content;
                comment.Rating = rating;
                comment.CreatedDate = DateTime.Now;

                _context.Comments.Update(comment);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Comment {commentId} updated by {userName} for course {id}.");
                TempData["Success"] = "Bình luận đã được chỉnh sửa thành công!";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to update comment {commentId} by {userName} for course {id}: {ex.Message}");
                TempData["Error"] = "Lỗi khi chỉnh sửa bình luận.";
            }

            return RedirectToAction("Details", new { id });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComment(string id, string commentId)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(commentId))
            {
                TempData["Error"] = "Thông tin xóa không hợp lệ.";
                return RedirectToAction("Details", new { id });
            }

            var comment = await _context.Comments
                .FirstOrDefaultAsync(c => c.CommentId == commentId && c.CourseId == id);

            if (comment == null)
            {
                _logger.LogWarning($"Comment with ID {commentId} not found for course {id}.");
                TempData["Error"] = "Không tìm thấy bình luận.";
                return RedirectToAction("Details", new { id });
            }

            var userName = User.Identity.Name;
            if (comment.UserName != userName)
            {
                _logger.LogWarning($"User {userName} attempted to delete comment {commentId} that they do not own.");
                TempData["Error"] = "Bạn không có quyền xóa bình luận này.";
                return RedirectToAction("Details", new { id });
            }

            try
            {
                _context.Comments.Remove(comment);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Comment {commentId} deleted by {userName} for course {id}.");
                TempData["Success"] = "Bình luận đã được xóa thành công!";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to delete comment {commentId} by {userName} for course {id}: {ex.Message}");
                TempData["Error"] = "Lỗi khi xóa bình luận.";
            }

            return RedirectToAction("Details", new { id });
        }

        [Authorize]
        public async Task<IActionResult> AllTests()
        {
            var userName = User.Identity.Name;
            if (string.IsNullOrEmpty(userName))
            {
                return Unauthorized("Bạn cần đăng nhập để xem danh sách bài kiểm tra.");
            }

            var courses = await _context.Courses
                .Include(c => c.Assignments)
                    .ThenInclude(a => a.Questions)
                        .ThenInclude(q => q.Options)
                .Include(c => c.Lessons)
                .ToListAsync();

            var enrollments = await _context.Enrollments
                .Where(e => e.UserName == userName)
                .ToListAsync();

            // Truy vấn submissions với điều kiện bỏ qua bản ghi không hợp lệ
            // Chỉ load submissions có SubmittedDate hợp lệ (lớn hơn DateTime.MinValue)
            var submissions = await _context.AssignmentSubmissions
                .Include(s => s.Assignment)
                .Include(s => s.Question)
                .Where(s => s.UserName == userName
                         && s.SubmittedDate > DateTime.MinValue
                         && s.AssignmentId != null
                         && s.QuestionId != null)
                .ToListAsync();

            if (submissions == null || !submissions.Any())
            {
                _logger.LogWarning("No valid submissions found for user {UserName}", userName);
            }
            else
            {
                _logger.LogInformation("Loaded {Count} submissions for user {UserName}", submissions.Count, userName);
                foreach (var submission in submissions)
                {
                    // Log SelectedOptionLabel và SelectedOptionText thay vì SubmissionContent
                    string selectedOptionLabel = submission.SelectedOptionLabel ?? "null";
                    string selectedOptionText = submission.SelectedOptionText ?? "null";
                    _logger.LogInformation("Submission: Id={SubmissionId}, SubmittedDate={SubmittedDate}, SelectedOptionLabel={SelectedOptionLabel}, SelectedOptionText={SelectedOptionText}",
                        submission.SubmissionId,
                        submission.SubmittedDate,
                        selectedOptionLabel,
                        selectedOptionText);
                }
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);

            var viewModel = new HomeViewModel
            {
                User = user,
                Courses = courses.Select(c => new CourseListViewModel
                {
                    CourseId = c.CourseId,
                    CourseName = c.CourseName,
                    Description = c.Description,
                    CreatedDate = c.CreatedDate,
                    ImageUrl = c.ImageUrl,
                    Price = c.Price,
                    IsEnrolled = enrollments.Any(e => e.CourseId == c.CourseId),
                    Lessons = c.Lessons ?? new List<Lesson>(),
                    Assignments = c.Assignments ?? new List<Assignment>(),
                    Level = c.Level,
                    DurationMinutes = c.DurationMinutes,
                    TagNames = c.CourseTags != null
                        ? c.CourseTags
                            .Select(ct => ct.Tag != null ? ct.Tag.Name : string.Empty)
                            .Where(name => !string.IsNullOrWhiteSpace(name))
                            .ToList()
                        : new List<string>()
                }).ToList(),
                SearchQuery = null,
                Enrollments = enrollments,
                Submissions = submissions ?? new List<AssignmentSubmission>()
            };

            return View("AllTests", viewModel);
        }

        [Authorize]
        public async Task<IActionResult> TakeTest(string id, string assignmentId)
        {
            var userName = User.Identity.Name;
            if (string.IsNullOrEmpty(userName))
            {
                return Unauthorized("Bạn cần đăng nhập để làm bài kiểm tra.");
            }

            var assignment = await _context.Assignments
                .Include(a => a.Questions)
                    .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(a => a.AssignmentId == assignmentId && a.CourseId == id);

            if (assignment == null)
            {
                return NotFound("Không tìm thấy bài kiểm tra.");
            }

            // Kiểm tra xem người dùng đã submit bài chưa
            var existingSubmissions = await _context.AssignmentSubmissions
                .Where(s => s.AssignmentId == assignmentId && s.UserName == userName)
                .ToListAsync();

            // Nếu đã có submission và đã submit (có SubmittedDate hợp lệ), không cho làm lại
            // Kiểm tra xem có ít nhất một submission với SubmittedDate hợp lệ (đã submit)
            var hasSubmitted = existingSubmissions.Any(s => 
                s.SubmittedDate > DateTime.MinValue);
            
            if (hasSubmitted)
            {
                TempData["Error"] = "Bạn đã hoàn thành bài kiểm tra này. Không thể làm lại.";
                return RedirectToAction("ViewTestResult", new { assignmentId = assignmentId, id = id });
            }

            var viewModel = new TakeTestViewModel
            {
                CourseId = id,
                AssignmentId = assignmentId,
                AssignmentTitle = assignment.Title,
                DurationInMinutes = assignment.DurationMinutes ?? 60, // Sử dụng giá trị từ DB, mặc định 60 nếu null
                Questions = assignment.Questions,
                SelectedAnswers = new Dictionary<string, string>()
            };

            ViewBag.HasExistingSubmissions = existingSubmissions.Any();
            ViewBag.AssignmentId = assignmentId;

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> ViewTestResult(string assignmentId, string id)
        {
            if (string.IsNullOrEmpty(assignmentId) || string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "ID bài kiểm tra hoặc khóa học không hợp lệ.";
                return RedirectToAction("AllTests");
            }

            var userName = User.Identity.Name;
            if (string.IsNullOrEmpty(userName))
            {
                TempData["Error"] = "Bạn cần đăng nhập để xem kết quả.";
                return RedirectToAction("AllTests");
            }

            var assignment = await _context.Assignments
                .Include(a => a.Questions)
                    .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(a => a.AssignmentId == assignmentId);

            if (assignment == null)
            {
                TempData["Error"] = "Không tìm thấy bài kiểm tra.";
                return RedirectToAction("AllTests");
            }

            var model = new ViewTestResultViewModel
            {
                AssignmentId = assignmentId,
                CourseId = id,
                AssignmentTitle = assignment.Title,
                Questions = assignment.Questions.ToList(),
                Submissions = await _context.AssignmentSubmissions
                    .Where(s => s.AssignmentId == assignmentId && s.UserName == userName)
                    .ToListAsync()
            };

            return View(model);
        }

        [Authorize(Roles = "Admin,Instructor")]
        // GET: Course/ManageCourses
        public async Task<IActionResult> ManageCourses(int page = 1)
        {
            _logger.LogInformation("ManageCourses GET called.");

            try
            {
                var courses = await _context.Courses
                    .Include(c => c.CourseInstructors)
                        .ThenInclude(ci => ci.User)
                            .ThenInclude(u => u.Role)
                    .ToListAsync();

                if (User.IsInRole("Instructor"))
                {
                    var instructorUserName = User.Identity.Name;
                    courses = courses
                        .Where(c => c.CourseInstructors.Any(ci => ci.User.UserName == instructorUserName && ci.User.Role.RoleName == "Instructor"))
                        .ToList();
                }
                // Admin có thể xem tất cả khóa học, không cần lọc

                int pageSize = 10;
                int totalCourses = courses.Count();
                int totalPages = (int)Math.Ceiling((double)totalCourses / pageSize);
                var pagedCourses = courses.Skip((page - 1) * pageSize).Take(pageSize).ToList();
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;

                return View("~/Views/Course/ManageCourses.cshtml", pagedCourses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving courses in ManageCourses.");
                TempData["Error"] = "Đã xảy ra lỗi khi lấy danh sách khóa học. Vui lòng thử lại.";
                return RedirectToAction("Index", "Home");
            }
        }
        [Authorize(Roles = "Admin,Instructor")]
        // GET: Course/CreateCourse
        public IActionResult CreateCourse()
        {
            _logger.LogInformation("CreateCourse GET called.");

            var model = new Course();
            model.SelectedTagIds = new List<string>();
            if (User.IsInRole("Admin"))
            {
                // Admin có thể chọn giảng viên
                var instructors = _context.Users
                    .Include(u => u.Role)
                    .Where(u => u.Role.RoleName == "Instructor")
                    .ToList();
                ViewBag.Instructors = instructors;
            }
            else if (User.IsInRole("Instructor"))
            {
                // Instructor chỉ có thể tạo khóa học cho chính mình
                var currentInstructor = _userRepository.GetByUserName(User.Identity.Name);
                ViewBag.CurrentInstructor = currentInstructor;
            }

            ViewBag.AllTags = GetActiveTags();

            return View("~/Views/Course/CreateCourse.cshtml", model);
        }
        // POST: Course/CreateCourse
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCourse(Course model, IFormFile imageFile, string instructorUserName, List<string> selectedTagIds)
        {
            _logger.LogInformation("CreateCourse POST called.");

            model.CourseId = Guid.NewGuid().ToString();
            model.CreatedDate = DateTime.Now;
            model.SelectedTagIds = selectedTagIds ?? new List<string>();

            // Xử lý upload hình ảnh
            if (imageFile != null && imageFile.Length > 0)
            {
                try
                {
                    model.ImageUrl = await SaveImageFile(imageFile);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi lưu file ảnh cho khóa học {CourseId}", model.CourseId);
                    ModelState.AddModelError("imageFile", "Lỗi khi tải lên hình ảnh. Vui lòng thử lại.");
                }
            }
            else
            {
                model.ImageUrl = null;
            }

            // Xử lý giảng viên
            if (User.IsInRole("Admin") && !string.IsNullOrEmpty(instructorUserName))
            {
                var instructor = _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefault(u => u.UserName == instructorUserName && u.Role.RoleName == "Instructor");

                if (instructor == null)
                {
                    ModelState.AddModelError("instructorUserName", "Giảng viên không hợp lệ hoặc không tồn tại.");
                }
                else
                {
                    model.CourseInstructors = new List<CourseInstructor>
                {
                    new CourseInstructor
                    {
                        CourseId = model.CourseId,
                        UserName = instructorUserName,
                        Course = model,
                        User = instructor
                    }
                };
                }
            }
            else if (User.IsInRole("Instructor"))
            {
                var instructorUserNameCurrent = User.Identity.Name;
                var instructor = _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefault(u => u.UserName == instructorUserNameCurrent && u.Role.RoleName == "Instructor");

                if (instructor == null)
                {
                    ModelState.AddModelError("instructorUserName", "Giảng viên không hợp lệ hoặc không tồn tại.");
                }
                else
                {
                    model.CourseInstructors = new List<CourseInstructor>
                {
                    new CourseInstructor
                    {
                        CourseId = model.CourseId,
                        UserName = instructorUserNameCurrent,
                        Course = model,
                        User = instructor
                    }
                };
                }
            }
            else
            {
                model.CourseInstructors = null;
            }

            // Xử lý tag
            if (model.SelectedTagIds.Any())
            {
                var tags = await _context.Tags
                    .Where(t => model.SelectedTagIds.Contains(t.TagId))
                    .ToListAsync();
                model.CourseTags = tags
                    .Select(tag => new CourseTag
                    {
                        CourseId = model.CourseId,
                        TagId = tag.TagId,
                        Tag = tag
                    }).ToList();
            }
            else
            {
                model.CourseTags = new List<CourseTag>();
            }

            // Xác thực lại model
            ModelState.Clear();
            TryValidateModel(model);

            if (ModelState.IsValid)
            {
                try
                {
                    _courseRepository.Add(model);
                    _courseRepository.Save();
                    TempData["Success"] = "Thêm khóa học thành công.";
                    _logger.LogInformation($"Course {model.CourseId} created successfully.");

                    return RedirectToAction("ManageCourses");
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

            // Tải lại dữ liệu cho View
            if (User.IsInRole("Admin"))
            {
                var instructors = _context.Users
                    .Include(u => u.Role)
                    .Where(u => u.Role.RoleName == "Instructor")
                    .ToList();
                ViewBag.Instructors = instructors;
            }
            else if (User.IsInRole("Instructor"))
            {
                var currentInstructor = _userRepository.GetByUserName(User.Identity.Name);
                ViewBag.CurrentInstructor = currentInstructor;
            }

            ViewBag.AllTags = GetActiveTags();

            return View("~/Views/Course/CreateCourse.cshtml", model);
        }

        // GET: Course/EditCourse/{id}
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> EditCourse(string id)
        {
            _logger.LogInformation($"EditCourse GET called with CourseId: {id}");

            if (string.IsNullOrEmpty(id))
            {
                _logger.LogWarning("CourseId is null or empty.");
                return NotFound();
            }

            var course = await _context.Courses
                .Include(c => c.CourseInstructors)
                    .ThenInclude(ci => ci.User)
                .Include(c => c.CourseTags)
                    .ThenInclude(ct => ct.Tag)
                .FirstOrDefaultAsync(c => c.CourseId == id);

            if (course == null)
            {
                _logger.LogWarning($"Course with CourseId: {id} not found.");
                return NotFound();
            }

            if (User.IsInRole("Instructor"))
            {
                var instructorUserName = User.Identity.Name;
                if (!course.CourseInstructors.Any(ci => ci.UserName == instructorUserName))
                {
                    TempData["Error"] = "Bạn không có quyền chỉnh sửa khóa học này.";
                    return RedirectToAction("ManageCourses");
                }
            }

            if (User.IsInRole("Admin"))
            {
                var instructors = _context.Users
                    .Include(u => u.Role)
                    .Where(u => u.Role.RoleName == "Instructor")
                    .ToList();
                ViewBag.Instructors = instructors;
            }
            else if (User.IsInRole("Instructor"))
            {
                var currentInstructor = _userRepository.GetByUserName(User.Identity.Name);
                ViewBag.CurrentInstructor = currentInstructor;
            }

            course.SelectedTagIds = course.CourseTags.Select(ct => ct.TagId).ToList();
            ViewBag.AllTags = GetActiveTags();

            return View("~/Views/Course/EditCourse.cshtml", course);
        }

        // POST: Course/EditCourse/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCourse(string id, Course model, IFormFile imageFile, string instructorUserName, List<string> selectedTagIds)
        {
            _logger.LogInformation($"EditCourse POST called with CourseId: {id}");

            if (id != model.CourseId)
            {
                _logger.LogWarning($"CourseId mismatch: {id} != {model.CourseId}");
                return NotFound();
            }

            var course = await _context.Courses
                .Include(c => c.CourseInstructors)
                    .ThenInclude(ci => ci.User)
                .Include(c => c.CourseTags)
                .FirstOrDefaultAsync(c => c.CourseId == id);

            if (course == null)
            {
                _logger.LogWarning($"Course with CourseId: {id} not found.");
                return NotFound();
            }

            if (User.IsInRole("Instructor"))
            {
                var currentUserName = User.Identity.Name;
                if (!course.CourseInstructors.Any(ci => ci.UserName == currentUserName))
                {
                    TempData["Error"] = "Bạn không có quyền chỉnh sửa khóa học này.";
                    return RedirectToAction("ManageCourses");
                }
            }

            // Cập nhật thông tin khóa học
            course.CourseName = model.CourseName;
            course.Description = model.Description;
            course.Price = model.Price;
            course.CreatedDate = model.CreatedDate;
            course.Level = model.Level;
            course.DurationMinutes = model.DurationMinutes;

            // Xử lý hình ảnh
            if (imageFile != null && imageFile.Length > 0)
            {
                try
                {
                    // Xóa ảnh cũ
                    if (!string.IsNullOrEmpty(course.ImageUrl) && course.ImageUrl != "/images/default-course-image.jpg")
                    {
                        var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", course.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                            _logger.LogInformation($"Old image deleted: {oldFilePath}");
                        }
                    }

                    course.ImageUrl = await SaveImageFile(imageFile);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error handling image for course {course.CourseId}");
                    ModelState.AddModelError("imageFile", "Lỗi khi tải lên hình ảnh. Vui lòng thử lại.");
                }
            }
            else if (string.IsNullOrEmpty(course.ImageUrl))
            {
                course.ImageUrl = "/images/default-course-image.jpg";
            }

            // Xử lý giảng viên (chỉ Admin mới có thể thay đổi giảng viên)
            if (User.IsInRole("Admin") && !string.IsNullOrEmpty(instructorUserName))
            {
                var instructor = _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefault(u => u.UserName == instructorUserName && u.Role.RoleName == "Instructor");

                if (instructor == null)
                {
                    ModelState.AddModelError("instructorUserName", "Giảng viên không hợp lệ hoặc không tồn tại.");
                }
                else
                {
                    var existingInstructor = course.CourseInstructors?.FirstOrDefault();
                    if (existingInstructor != null)
                    {
                        _context.CourseInstructors.Remove(existingInstructor);
                    }

                    course.CourseInstructors = new List<CourseInstructor>
                {
                    new CourseInstructor
                    {
                        CourseId = course.CourseId,
                        UserName = instructorUserName,
                        Course = course,
                        User = instructor
                    }
                };
                }
            }

            var normalizedTagIds = selectedTagIds?.Where(id => !string.IsNullOrWhiteSpace(id))
                .Select(id => id.Trim()).Distinct().ToList() ?? new List<string>();
            var existingCourseTags = await _context.CourseTags
                .Where(ct => ct.CourseId == course.CourseId)
                .ToListAsync();
            var toRemove = existingCourseTags.Where(ct => !normalizedTagIds.Contains(ct.TagId)).ToList();
            if (toRemove.Any())
            {
                _context.CourseTags.RemoveRange(toRemove);
            }
            var existingIds = existingCourseTags.Select(ct => ct.TagId).ToHashSet();
            foreach (var tagId in normalizedTagIds)
            {
                if (!existingIds.Contains(tagId))
                {
                    _context.CourseTags.Add(new CourseTag
                    {
                        CourseId = course.CourseId,
                        TagId = tagId
                    });
                }
            }
            course.SelectedTagIds = normalizedTagIds;

            // Xác thực lại model
            ModelState.Clear();
            TryValidateModel(course);

            if (ModelState.IsValid)
            {
                try
                {
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Chỉnh sửa khóa học thành công.";
                    _logger.LogInformation($"Course {course.CourseId} updated successfully.");
                    return RedirectToAction("ManageCourses");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error updating course: {course.CourseId}");
                    TempData["Error"] = $"Đã xảy ra lỗi khi chỉnh sửa khóa học: {ex.Message}";
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                _logger.LogWarning("Model state is invalid for editing course. Errors: {0}", string.Join(", ", errors));
                TempData["Error"] = $"Vui lòng kiểm tra lại thông tin: {string.Join(", ", errors)}";
            }

            // Tải lại dữ liệu cho View
            if (User.IsInRole("Admin"))
            {
                var instructors = _context.Users
                    .Include(u => u.Role)
                    .Where(u => u.Role.RoleName == "Instructor")
                    .ToList();
                ViewBag.Instructors = instructors;
            }
            else if (User.IsInRole("Instructor"))
            {
                var currentInstructor = _userRepository.GetByUserName(User.Identity.Name);
                ViewBag.CurrentInstructor = currentInstructor;
            }

            ViewBag.AllTags = GetActiveTags();

            return View("~/Views/Course/EditCourse.cshtml", course);
        }

        // POST: Course/DeleteCourse/{id}
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

            if (User.IsInRole("Instructor"))
            {
                var currentUserName = User.Identity.Name;
                var courseWithInstructors = _context.Courses
                    .Include(c => c.CourseInstructors)
                    .FirstOrDefault(c => c.CourseId == id);

                if (!courseWithInstructors.CourseInstructors.Any(ci => ci.UserName == currentUserName))
                {
                    TempData["Error"] = "Bạn không có quyền xóa khóa học này.";
                    return RedirectToAction("ManageCourses");
                }
            }

            try
            {
                // Xóa hình ảnh
                if (!string.IsNullOrEmpty(course.ImageUrl) && course.ImageUrl != "/images/default-course-image.jpg")
                {
                    var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", course.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                        _logger.LogInformation($"Image deleted successfully: {imagePath}");
                    }
                    else
                    {
                        _logger.LogWarning($"Image file not found: {imagePath}");
                    }
                }

                _courseRepository.Delete(id);
                TempData["Success"] = "Xóa khóa học thành công.";
                _logger.LogInformation($"Course {id} deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting course: {id}. Details: {ex.Message}");
                TempData["Error"] = $"Đã xảy ra lỗi khi xóa khóa học: {ex.Message}. Vui lòng kiểm tra lại.";
            }

            return RedirectToAction("ManageCourses");
        }

        private List<Tag> GetActiveTags()
        {
            return _context.Tags
                .Where(t => t.IsActive)
                .OrderBy(t => t.Category)
                .ThenBy(t => t.Name)
                .ToList();
        }

        // Phương thức hỗ trợ lưu file ảnh
        private async Task<string> SaveImageFile(IFormFile imageFile)
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
            return $"/images/{fileName}";
        }
    }
 }