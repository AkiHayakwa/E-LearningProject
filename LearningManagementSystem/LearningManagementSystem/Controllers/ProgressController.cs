using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using LearningManagementSystem.Models;
using LearningManagementSystem.Repositories;

namespace LearningManagementSystem.Controllers
{
    [Authorize]
    public class ProgressController : Controller
    {
        private readonly IProgressRepository _progressRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly ILogger<ProgressController> _logger;

        public ProgressController(IProgressRepository progressRepository, ILessonRepository lessonRepository, ILogger<ProgressController> logger)
        {
            _progressRepository = progressRepository;
            _lessonRepository = lessonRepository;
            _logger = logger;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProgress(string lessonId, bool completionStatus)
        {
            _logger.LogInformation($"UpdateProgress called with LessonId: {lessonId}, CompletionStatus: {completionStatus}");

            if (string.IsNullOrEmpty(lessonId))
            {
                _logger.LogWarning("LessonId is empty.");
                return Json(new { success = false, error = "LessonId không được để trống" });
            }

            var lesson = _lessonRepository.GetById(lessonId);
            if (lesson == null)
            {
                _logger.LogWarning($"Lesson with LessonId: {lessonId} not found.");
                return Json(new { success = false, error = "Không tìm thấy bài học" });
            }

            var userName = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(userName))
            {
                _logger.LogError("UserName could not be determined from claims.");
                return Json(new { success = false, error = "Bạn cần đăng nhập" });
            }

            try
            {
                var progress = _progressRepository.GetByUserAndLesson(userName, lessonId);

                if (progress == null)
                {
                    progress = new Progress
                    {
                        ProgressId = Guid.NewGuid().ToString(),
                        UserName = userName,
                        LessonId = lessonId,
                        CompletionStatus = completionStatus,
                        CompletionDate = completionStatus ? DateTime.Now : null
                    };
                    _logger.LogInformation($"Creating new Progress record: ProgressId={progress.ProgressId}, UserName={progress.UserName}, LessonId={progress.LessonId}, CompletionStatus={progress.CompletionStatus}");
                    _progressRepository.Add(progress);
                }
                else
                {
                    if (progress.CompletionStatus != completionStatus)
                    {
                        progress.CompletionStatus = completionStatus;
                        progress.CompletionDate = completionStatus ? DateTime.Now : null;
                        _logger.LogInformation($"Updating existing Progress record: ProgressId={progress.ProgressId}, UserName={progress.UserName}, LessonId={progress.LessonId}, CompletionStatus={progress.CompletionStatus}");
                        _progressRepository.Update(progress);
                    }
                    else
                    {
                        _logger.LogInformation($"Progress record already exists with same CompletionStatus for UserName: {userName}, LessonId: {lessonId}");
                        return Json(new { success = true }); // Trả về success ngay cả khi không cần cập nhật
                    }
                }

                await _progressRepository.SaveAsync();

                _logger.LogInformation($"User {userName} updated progress for LessonId: {lessonId}, CompletionStatus: {completionStatus}");
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating progress for UserName: {userName}, LessonId: {lessonId}. Error: {ex.Message}, StackTrace: {ex.StackTrace}");
                return Json(new { success = false, error = ex.Message });
            }
        }
    }
}