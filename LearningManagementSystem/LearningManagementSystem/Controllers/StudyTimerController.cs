using LearningManagementSystem.Data;
using LearningManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace LearningManagementSystem.Controllers
{
    [Authorize(Roles = "Student")]
    [ApiController]
    [Route("[controller]/[action]")]
    public class StudyTimerController : ControllerBase
    {
        private readonly LMSContext _context;
        private readonly ILogger<StudyTimerController> _logger;

        public StudyTimerController(LMSContext context, ILogger<StudyTimerController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public class LogSessionRequest
        {
            public string CourseId { get; set; } = null!;
            public string? LessonId { get; set; }
            public int DurationSeconds { get; set; }
            public DateTime? StartedAt { get; set; }
            public DateTime? EndedAt { get; set; }
            public string? Note { get; set; }
        }

        public class StudyStatsResponse
        {
            public int TotalSeconds { get; set; }
            public int WeeklySeconds { get; set; }
            public int CurrentStreakDays { get; set; }
            public DateTime? LastStudiedAt { get; set; }
            public int TotalSessions { get; set; }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogSession([FromBody] LogSessionRequest request)
        {
            if (request == null)
            {
                return BadRequest("Thiếu dữ liệu phiên học.");
            }

            if (string.IsNullOrWhiteSpace(request.CourseId))
            {
                return BadRequest("CourseId không được để trống.");
            }

            if (request.DurationSeconds <= 0)
            {
                return BadRequest("Thời lượng học phải lớn hơn 0.");
            }

            var userName = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(userName))
            {
                return Unauthorized("Bạn cần đăng nhập.");
            }

            var course = await _context.Courses.AsNoTracking().FirstOrDefaultAsync(c => c.CourseId == request.CourseId);
            if (course == null)
            {
                return NotFound("Không tìm thấy khóa học.");
            }

            var startedAt = request.StartedAt?.ToUniversalTime() ?? DateTime.UtcNow.AddSeconds(-request.DurationSeconds);
            var endedAt = request.EndedAt?.ToUniversalTime() ?? DateTime.UtcNow;

            if (endedAt < startedAt)
            {
                endedAt = startedAt.AddSeconds(request.DurationSeconds);
            }

            var session = new StudySession
            {
                CourseId = request.CourseId,
                LessonId = string.IsNullOrWhiteSpace(request.LessonId) ? null : request.LessonId,
                UserName = userName,
                StartedAt = startedAt,
                EndedAt = endedAt,
                DurationSeconds = request.DurationSeconds,
                Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim()
            };

            try
            {
                _context.StudySessions.Add(session);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lưu phiên học cho {User}", userName);
                return StatusCode(500, "Không thể lưu phiên học.");
            }

            var stats = await CalculateStatsAsync(userName, request.CourseId);
            return Ok(stats);
        }

        [HttpGet]
        public async Task<IActionResult> GetStats(string courseId)
        {
            if (string.IsNullOrWhiteSpace(courseId))
            {
                return BadRequest("CourseId không được để trống.");
            }

            var userName = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(userName))
            {
                return Unauthorized("Bạn cần đăng nhập.");
            }

            var stats = await CalculateStatsAsync(userName, courseId);
            return Ok(stats);
        }

        private async Task<StudyStatsResponse> CalculateStatsAsync(string userName, string courseId)
        {
            var sessions = await _context.StudySessions
                .Where(ss => ss.UserName == userName && ss.CourseId == courseId)
                .OrderByDescending(ss => ss.StartedAt)
                .ToListAsync();

            var totalSeconds = sessions.Sum(ss => ss.DurationSeconds);
            var weekStart = DateTime.UtcNow.Date.AddDays(-6);
            var weeklySeconds = sessions
                .Where(ss => ss.StartedAt.Date >= weekStart)
                .Sum(ss => ss.DurationSeconds);

            var lastStudied = sessions.FirstOrDefault()?.EndedAt ?? sessions.FirstOrDefault()?.StartedAt;
            var streak = CalculateStreak(sessions);

            return new StudyStatsResponse
            {
                TotalSeconds = totalSeconds,
                WeeklySeconds = weeklySeconds,
                CurrentStreakDays = streak,
                LastStudiedAt = lastStudied,
                TotalSessions = sessions.Count
            };
        }

        private static int CalculateStreak(IEnumerable<StudySession> sessions)
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
    }
}

