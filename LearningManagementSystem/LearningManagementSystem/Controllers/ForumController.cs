using LearningManagementSystem.Data;
using LearningManagementSystem.Models;
using LearningManagementSystem.Repositories;
using LearningManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;

namespace LearningManagementSystem.Controllers
{
    [Authorize]
    public class ForumController : Controller
    {
        private readonly IForumRepository _forumRepository;
        private readonly ITopicRepository _topicRepository;
        private readonly IPostRepository _postRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly IReportRepository _reportRepository;
        private readonly EmailService _emailService;
        private readonly LMSContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<ForumController> _logger;

        public ForumController(
            IForumRepository forumRepository,
            ITopicRepository topicRepository,
            IPostRepository postRepository,
            IEnrollmentRepository enrollmentRepository,
            INotificationRepository notificationRepository,
            IReportRepository reportRepository,
            EmailService emailService,
            LMSContext context,
            IWebHostEnvironment webHostEnvironment,
            ILogger<ForumController> logger)
        {
            _forumRepository = forumRepository;
            _topicRepository = topicRepository;
            _postRepository = postRepository;
            _enrollmentRepository = enrollmentRepository;
            _notificationRepository = notificationRepository;
            _reportRepository = reportRepository;
            _emailService = emailService;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        // GET: Forum/Index?courseId=xxx
        public async Task<IActionResult> Index(string courseId, string filter = "all", string sort = "newest", int page = 1)
        {
            if (string.IsNullOrEmpty(courseId))
            {
                return BadRequest("CourseId is required");
            }

            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userName))
            {
                return Unauthorized();
            }

            // Kiểm tra user đã đăng ký khóa học chưa
            var isEnrolled = await _enrollmentRepository.GetAll()
                .AnyAsync(e => e.UserName == userName && e.CourseId == courseId);

            // Lấy hoặc tạo forum cho khóa học
            var forum = _forumRepository.GetByCourseId(courseId);
            if (forum == null)
            {
                forum = new Forum
                {
                    ForumId = Guid.NewGuid().ToString(),
                    CourseId = courseId,
                    Name = "Diễn đàn khóa học",
                    Description = "Nơi thảo luận về khóa học",
                    CreatedDate = DateTime.UtcNow,
                    IsActive = true
                };
                _forumRepository.Add(forum);
                _forumRepository.Save();
            }

            // Lấy danh sách topics
            var topicsQuery = _topicRepository.GetByForumId(forum.ForumId);

            // Filter
            switch (filter.ToLower())
            {
                case "unsolved":
                    topicsQuery = topicsQuery.Where(t => t.Status != "Solved");
                    break;
                case "solved":
                    topicsQuery = topicsQuery.Where(t => t.Status == "Solved");
                    break;
                case "my":
                    topicsQuery = topicsQuery.Where(t => t.UserName == userName);
                    break;
            }

            // Sort
            switch (sort.ToLower())
            {
                case "newest":
                    topicsQuery = topicsQuery.OrderByDescending(t => t.CreatedDate);
                    break;
                case "mostreplies":
                    topicsQuery = topicsQuery.OrderByDescending(t => t.ReplyCount);
                    break;
                case "mostviews":
                    topicsQuery = topicsQuery.OrderByDescending(t => t.ViewCount);
                    break;
                case "oldest":
                    topicsQuery = topicsQuery.OrderBy(t => t.CreatedDate);
                    break;
            }

            // Pagination
            int pageSize = 10;
            int totalTopics = await topicsQuery.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalTopics / pageSize);
            var topics = await topicsQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Thống kê cho Admin/Instructor
            var isInstructor = User.IsInRole("Instructor") || User.IsInRole("Admin");
            if (isInstructor)
            {
                var allTopics = _topicRepository.GetByForumId(forum.ForumId);
                var totalTopicsCount = await allTopics.CountAsync();
                var solvedTopics = await allTopics.Where(t => t.Status == "Solved").CountAsync();
                var unsolvedTopics = await allTopics.Where(t => t.Status != "Solved" && t.Status != "Locked").CountAsync();
                var pinnedTopics = await allTopics.Where(t => t.Status == "Pinned").CountAsync();
                var lockedTopics = await allTopics.Where(t => t.Status == "Locked").CountAsync();
                var totalReplies = await allTopics.SumAsync(t => (int?)t.ReplyCount) ?? 0;
                var totalViews = await allTopics.SumAsync(t => (int?)t.ViewCount) ?? 0;
                var pendingReports = await _reportRepository.GetAll()
                    .Where(r => r.Status == "Pending" && r.TopicId != null && allTopics.Any(t => t.TopicId == r.TopicId))
                    .CountAsync();

                ViewBag.IsInstructor = true;
                ViewBag.TotalTopicsCount = totalTopicsCount;
                ViewBag.SolvedTopics = solvedTopics;
                ViewBag.UnsolvedTopics = unsolvedTopics;
                ViewBag.PinnedTopics = pinnedTopics;
                ViewBag.LockedTopics = lockedTopics;
                ViewBag.TotalReplies = totalReplies;
                ViewBag.TotalViews = totalViews;
                ViewBag.PendingReports = pendingReports;
            }
            else
            {
                ViewBag.IsInstructor = false;
            }

            // Lấy popular tags cho sidebar
            var popularTags = await _context.TopicTags
                .Where(tt => tt.Topic != null && tt.Topic.ForumId == forum.ForumId)
                .GroupBy(tt => tt.TagName)
                .Select(g => new { TagName = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToListAsync();

            // Lấy popular topics cho sidebar (most views)
            var popularTopics = await _topicRepository.GetByForumId(forum.ForumId)
                .OrderByDescending(t => t.ViewCount)
                .Take(5)
                .Select(t => new { t.TopicId, t.Title, t.ViewCount, t.ReplyCount })
                .ToListAsync();

            // Lấy thông tin user cho create post section
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);

            ViewBag.CourseId = courseId;
            ViewBag.ForumId = forum.ForumId;
            ViewBag.IsEnrolled = isEnrolled;
            ViewBag.Filter = filter;
            ViewBag.Sort = sort;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalTopics = totalTopics;
            ViewBag.PopularTags = popularTags;
            ViewBag.PopularTopics = popularTopics;
            ViewBag.UserName = userName;
            ViewBag.User = user;

            return View(topics);
        }

        // GET: Forum/TopicDetail?id=xxx
        public async Task<IActionResult> TopicDetail(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("TopicId is required");
            }

            var topic = _topicRepository.GetById(id);
            if (topic == null)
            {
                return NotFound("Topic not found");
            }

            var userName = User.FindFirst(ClaimTypes.Name)?.Value;

            // Tăng view count
            topic.ViewCount++;
            _topicRepository.Update(topic);
            _topicRepository.Save();

            // Lấy posts (chỉ lấy posts chính)
            var posts = await _postRepository.GetByTopicId(id)
                .OrderBy(p => p.CreatedDate)
                .ToListAsync();

            foreach (var post in posts)
            {
                await LoadRepliesRecursive(post);
            }

            // Lấy popular tags cho sidebar
            var popularTags = await _context.TopicTags
                .Where(tt => tt.Topic != null && tt.Topic.ForumId == topic.ForumId)
                .GroupBy(tt => tt.TagName)
                .Select(g => new { TagName = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToListAsync();

            // Lấy thông tin user
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);

            ViewBag.Topic = topic;
            ViewBag.Posts = posts;
            ViewBag.UserName = userName;
            ViewBag.IsInstructor = User.IsInRole("Instructor") || User.IsInRole("Admin");
            ViewBag.IsTopicOwner = topic.UserName == userName;
            ViewBag.PopularTags = popularTags;
            ViewBag.User = user;
            ViewBag.CourseId = topic.Forum?.CourseId;
            ViewBag.Sort = null; // Set default sort for sidebar

            return View();
        }

        // GET: Forum/CreateTopic?courseId=xxx
        [HttpGet]
        public async Task<IActionResult> CreateTopic(string courseId)
        {
            if (string.IsNullOrEmpty(courseId))
            {
                return BadRequest("CourseId is required");
            }

            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userName))
            {
                return Unauthorized();
            }

            // Kiểm tra user đã đăng ký khóa học chưa
            var isEnrolled = await _enrollmentRepository.GetAll()
                .AnyAsync(e => e.UserName == userName && e.CourseId == courseId);

            var isInstructor = User.IsInRole("Instructor") || User.IsInRole("Admin");
            var isCourseInstructor = await _context.CourseInstructors
                .AnyAsync(ci => ci.CourseId == courseId && ci.UserName == userName);

            if (!isEnrolled && !isInstructor && !isCourseInstructor)
            {
                TempData["Error"] = "Bạn cần đăng ký khóa học để tạo chủ đề thảo luận.";
                return RedirectToAction("Details", "Course", new { id = courseId });
            }

            ViewBag.CourseId = courseId;
            return View();
        }

        // POST: Forum/CreateTopic
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTopic(string courseId, Topic topic, string[]? tags, List<IFormFile>? images)
        {
            if (string.IsNullOrEmpty(courseId) || topic == null)
            {
                TempData["Error"] = "Thông tin không hợp lệ.";
                return RedirectToAction("Index", new { courseId });
            }

            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userName))
            {
                return Unauthorized();
            }

            // Lấy hoặc tạo forum
            var forum = _forumRepository.GetByCourseId(courseId);
            if (forum == null)
            {
                forum = new Forum
                {
                    ForumId = Guid.NewGuid().ToString(),
                    CourseId = courseId,
                    Name = "Diễn đàn khóa học",
                    Description = "Nơi thảo luận về khóa học",
                    CreatedDate = DateTime.UtcNow,
                    IsActive = true
                };
                _forumRepository.Add(forum);
                _forumRepository.Save();
            }

            // Tạo topic
            topic.TopicId = Guid.NewGuid().ToString();
            topic.ForumId = forum.ForumId;
            topic.UserName = userName;
            topic.CreatedDate = DateTime.UtcNow;
            topic.Status = "New";
            topic.ViewCount = 0;
            topic.ReplyCount = 0;
            topic.IsActive = true;

            // Thêm tags nếu có
            if (tags != null && tags.Length > 0)
            {
                topic.TopicTags = tags
                    .Where(t => !string.IsNullOrWhiteSpace(t))
                    .Select(tagName => new TopicTag
                    {
                        TagId = Guid.NewGuid().ToString(),
                        TopicId = topic.TopicId,
                        TagName = tagName.Trim()
                    }).ToList();
            }

            // Xử lý upload hình ảnh và thêm vào content HTML
            if (images != null && images.Any())
            {
                var uploadDirectory = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "forum");
                if (!Directory.Exists(uploadDirectory))
                {
                    Directory.CreateDirectory(uploadDirectory);
                }

                var imagesHtml = "";
                foreach (var image in images)
                {
                    if (image != null && image.Length > 0 && image.ContentType.StartsWith("image/"))
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                        var filePath = Path.Combine(uploadDirectory, fileName);
                        var relativePath = $"/uploads/forum/{fileName}";

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await image.CopyToAsync(stream);
                        }

                        // Thêm hình ảnh vào cuối content HTML
                        imagesHtml += $"<div class=\"topic-uploaded-image\"><img src=\"{relativePath}\" alt=\"{image.FileName}\" class=\"topic-image\" onclick=\"openImageModal('{relativePath}')\" /></div>";
                    }
                }
                
                // Thêm hình ảnh vào cuối content (sau text)
                if (!string.IsNullOrEmpty(imagesHtml))
                {
                    topic.Content += imagesHtml;
                }
            }

            _topicRepository.Add(topic);
            _topicRepository.Save();

            // Gửi thông báo cho giảng viên nếu là câu hỏi
            var courseInstructors = await _context.CourseInstructors
                .Where(ci => ci.CourseId == courseId)
                .Select(ci => ci.UserName)
                .ToListAsync();

            foreach (var instructorUserName in courseInstructors)
            {
                var notification = new Notification
                {
                    NotificationId = Guid.NewGuid().ToString(),
                    UserName = instructorUserName,
                    Title = "Có chủ đề mới trong diễn đàn",
                    Content = $"Học viên {userName} đã tạo chủ đề mới: {topic.Title}",
                    CreatedDate = DateTime.UtcNow,
                    IsRead = false
                };
                await _notificationRepository.AddAsync(notification);

                // Gửi email notification (tùy chọn)
                try
                {
                    var instructor = await _context.Users.FindAsync(instructorUserName);
                    if (instructor != null && !string.IsNullOrEmpty(instructor.Email))
                    {
                        var emailBody = $@"
                            <h3>Có chủ đề mới trong diễn đàn</h3>
                            <p>Học viên <strong>{userName}</strong> đã tạo chủ đề mới trong khóa học của bạn:</p>
                            <p><strong>{topic.Title}</strong></p>
                            <p><a href='{Request.Scheme}://{Request.Host}/Forum/TopicDetail?id={topic.TopicId}'>Xem chủ đề</a></p>
                        ";
                        await _emailService.SendEmailAsync(instructor.Email, "Có chủ đề mới trong diễn đàn", emailBody);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send email notification to instructor {Instructor}", instructorUserName);
                    // Không throw exception để không ảnh hưởng đến quá trình tạo topic
                }
            }

            TempData["Success"] = "Tạo chủ đề thành công!";
            return RedirectToAction("TopicDetail", new { id = topic.TopicId });
        }

        // POST: Forum/CreatePost
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePost(string topicId, string content, string? parentPostId, List<IFormFile>? images)
        {
            if (string.IsNullOrEmpty(topicId) || string.IsNullOrEmpty(content))
            {
                return Json(new { success = false, message = "Nội dung không được để trống." });
            }

            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userName))
            {
                return Json(new { success = false, message = "Bạn cần đăng nhập." });
            }

            var topic = _topicRepository.GetById(topicId);
            if (topic == null)
            {
                return Json(new { success = false, message = "Chủ đề không tồn tại." });
            }

            // Tạo post
            var post = new Post
            {
                PostId = Guid.NewGuid().ToString(),
                TopicId = topicId,
                UserName = userName,
                Content = content.Trim(),
                ParentPostId = string.IsNullOrEmpty(parentPostId) ? null : parentPostId,
                LikeCount = 0,
                IsAnswer = false,
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            };

            _postRepository.Add(post);
            _postRepository.Save(); // Save để có PostId

            // Xử lý upload hình ảnh
            if (images != null && images.Any())
            {
                var uploadDirectory = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "forum");
                if (!Directory.Exists(uploadDirectory))
                {
                    Directory.CreateDirectory(uploadDirectory);
                }

                foreach (var image in images)
                {
                    if (image != null && image.Length > 0 && image.ContentType.StartsWith("image/"))
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                        var filePath = Path.Combine(uploadDirectory, fileName);
                        var relativePath = $"/uploads/forum/{fileName}";

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await image.CopyToAsync(stream);
                        }

                        var attachment = new PostAttachment
                        {
                            AttachmentId = Guid.NewGuid().ToString(),
                            PostId = post.PostId,
                            FileName = image.FileName,
                            FilePath = relativePath,
                            FileSize = image.Length,
                            FileType = image.ContentType,
                            UploadedDate = DateTime.UtcNow
                        };

                        _context.PostAttachments.Add(attachment);
                    }
                }
                await _context.SaveChangesAsync();
            }

            // Cập nhật topic
            topic.ReplyCount++;
            topic.LastReplyDate = DateTime.UtcNow;
            topic.LastReplyBy = userName;
            _topicRepository.Update(topic);
            _topicRepository.Save();

            // Gửi thông báo
            if (topic.UserName != userName) // Không gửi thông báo cho chính người tạo topic
            {
                var notification = new Notification
                {
                    NotificationId = Guid.NewGuid().ToString(),
                    UserName = topic.UserName,
                    Title = "Có phản hồi mới",
                    Content = $"{userName} đã phản hồi trong chủ đề: {topic.Title}",
                    CreatedDate = DateTime.UtcNow,
                    IsRead = false
                };
                await _notificationRepository.AddAsync(notification);

                // Gửi email notification (tùy chọn)
                try
                {
                    var topicOwner = await _context.Users.FindAsync(topic.UserName);
                    if (topicOwner != null && !string.IsNullOrEmpty(topicOwner.Email))
                    {
                        var emailBody = $@"
                            <h3>Có phản hồi mới</h3>
                            <p><strong>{userName}</strong> đã phản hồi trong chủ đề của bạn:</p>
                            <p><strong>{topic.Title}</strong></p>
                            <p><a href='{Request.Scheme}://{Request.Host}/Forum/TopicDetail?id={topic.TopicId}'>Xem phản hồi</a></p>
                        ";
                        await _emailService.SendEmailAsync(topicOwner.Email, "Có phản hồi mới trong diễn đàn", emailBody);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send email notification to topic owner {Owner}", topic.UserName);
                    // Không throw exception để không ảnh hưởng đến quá trình tạo post
                }
            }

            return Json(new { success = true, message = "Đăng phản hồi thành công!" });
        }

        // POST: Forum/LikePost
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LikePost(string postId)
        {
            if (string.IsNullOrEmpty(postId))
            {
                return Json(new { success = false, message = "PostId is required" });
            }

            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userName))
            {
                return Json(new { success = false, message = "Bạn cần đăng nhập." });
            }

            var post = _postRepository.GetById(postId);
            if (post == null)
            {
                return Json(new { success = false, message = "Post not found" });
            }

            // Không cho like bài của chính mình
            if (post.UserName == userName)
            {
                return Json(new { success = false, message = "Bạn không thể like bài viết của chính mình." });
            }

            // Kiểm tra đã like chưa
            var existingLike = await _context.PostLikes
                .FirstOrDefaultAsync(pl => pl.PostId == postId && pl.UserName == userName);

            if (existingLike != null)
            {
                // Unlike
                _context.PostLikes.Remove(existingLike);
                post.LikeCount = Math.Max(0, post.LikeCount - 1);
            }
            else
            {
                // Like
                var like = new PostLike
                {
                    LikeId = Guid.NewGuid().ToString(),
                    PostId = postId,
                    UserName = userName,
                    CreatedDate = DateTime.UtcNow
                };
                _context.PostLikes.Add(like);
                post.LikeCount++;
            }

            _postRepository.Update(post);
            _postRepository.Save();

            return Json(new { success = true, likeCount = post.LikeCount, isLiked = existingLike == null });
        }

        // POST: Forum/MarkAsSolved
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> MarkAsSolved(string topicId, string? answerPostId)
        {
            if (string.IsNullOrEmpty(topicId))
            {
                return Json(new { success = false, message = "TopicId is required" });
            }

            var topic = _topicRepository.GetById(topicId);
            if (topic == null)
            {
                return Json(new { success = false, message = "Topic not found" });
            }

            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userName))
            {
                return Json(new { success = false, message = "Bạn cần đăng nhập để thực hiện hành động này." });
            }

            // Chỉ cho phép người tạo topic hoặc Instructor/Admin đánh dấu giải quyết
            var isInstructor = User.IsInRole("Instructor") || User.IsInRole("Admin");
            var isTopicOwner = topic.UserName == userName;

            if (!isInstructor && !isTopicOwner)
            {
                return Json(new { success = false, message = "Chỉ người tạo chủ đề hoặc Instructor/Admin mới có quyền đánh dấu giải quyết." });
            }

            // Đánh dấu đã giải quyết
            if (topic.Status == "Solved")
            {
                topic.Status = "New";
                if (!string.IsNullOrEmpty(answerPostId))
                {
                    var post = _postRepository.GetById(answerPostId);
                    if (post != null)
                    {
                        post.IsAnswer = false;
                        _postRepository.Update(post);
                    }
                }
            }
            else
            {
                topic.Status = "Solved";
                if (!string.IsNullOrEmpty(answerPostId))
                {
                    var post = _postRepository.GetById(answerPostId);
                    if (post != null)
                    {
                        post.IsAnswer = true;
                        _postRepository.Update(post);
                    }
                }
            }

            _topicRepository.Update(topic);
            _topicRepository.Save();

            return Json(new { success = true, status = topic.Status });
        }

        // POST: Forum/ReportPost
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReportPost(string postId, string reason, string? description)
        {
            if (string.IsNullOrEmpty(postId) || string.IsNullOrEmpty(reason))
            {
                return Json(new { success = false, message = "Thiếu thông tin báo cáo." });
            }

            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userName))
            {
                return Json(new { success = false, message = "Bạn cần đăng nhập." });
            }

            var post = _postRepository.GetById(postId);
            if (post == null)
            {
                return Json(new { success = false, message = "Post not found" });
            }

            // Kiểm tra đã báo cáo chưa
            var existingReport = await _context.Reports
                .FirstOrDefaultAsync(r => r.PostId == postId && r.ReportedBy == userName && r.Status == "Pending");

            if (existingReport != null)
            {
                return Json(new { success = false, message = "Bạn đã báo cáo bài viết này rồi." });
            }

            var report = new Report
            {
                ReportId = Guid.NewGuid().ToString(),
                PostId = postId,
                ReportedBy = userName,
                Reason = reason,
                Description = description,
                Status = "Pending",
                CreatedDate = DateTime.UtcNow
            };

            _reportRepository.Add(report);
            _reportRepository.Save();

            return Json(new { success = true, message = "Báo cáo đã được gửi. Cảm ơn bạn đã phản hồi!" });
        }

        // POST: Forum/ReportTopic
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReportTopic(string topicId, string reason, string? description)
        {
            if (string.IsNullOrEmpty(topicId) || string.IsNullOrEmpty(reason))
            {
                return Json(new { success = false, message = "Thiếu thông tin báo cáo." });
            }

            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userName))
            {
                return Json(new { success = false, message = "Bạn cần đăng nhập." });
            }

            var topic = _topicRepository.GetById(topicId);
            if (topic == null)
            {
                return Json(new { success = false, message = "Topic not found" });
            }

            // Kiểm tra đã báo cáo chưa
            var existingReport = await _context.Reports
                .FirstOrDefaultAsync(r => r.TopicId == topicId && r.ReportedBy == userName && r.Status == "Pending");

            if (existingReport != null)
            {
                return Json(new { success = false, message = "Bạn đã báo cáo chủ đề này rồi." });
            }

            var report = new Report
            {
                ReportId = Guid.NewGuid().ToString(),
                TopicId = topicId,
                ReportedBy = userName,
                Reason = reason,
                Description = description,
                Status = "Pending",
                CreatedDate = DateTime.UtcNow
            };

            _reportRepository.Add(report);
            _reportRepository.Save();

            return Json(new { success = true, message = "Báo cáo đã được gửi. Cảm ơn bạn đã phản hồi!" });
        }

        // POST: Forum/PinTopic
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<IActionResult> PinTopic(string topicId)
        {
            if (string.IsNullOrEmpty(topicId))
            {
                return Json(new { success = false, message = "TopicId is required" });
            }

            var topic = _topicRepository.GetById(topicId);
            if (topic == null)
            {
                return Json(new { success = false, message = "Topic not found" });
            }

            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            var isInstructor = User.IsInRole("Instructor") || User.IsInRole("Admin");
            var isCourseInstructor = await _context.CourseInstructors
                .AnyAsync(ci => ci.CourseId == topic.Forum.CourseId && ci.UserName == userName);

            if (!isInstructor && !isCourseInstructor)
            {
                return Json(new { success = false, message = "Bạn không có quyền thực hiện hành động này." });
            }

            topic.Status = topic.Status == "Pinned" ? "New" : "Pinned";
            _topicRepository.Update(topic);
            _topicRepository.Save();

            return Json(new { success = true, status = topic.Status });
        }

        // POST: Forum/LockTopic
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<IActionResult> LockTopic(string topicId)
        {
            if (string.IsNullOrEmpty(topicId))
            {
                return Json(new { success = false, message = "TopicId is required" });
            }

            var topic = _topicRepository.GetById(topicId);
            if (topic == null)
            {
                return Json(new { success = false, message = "Topic not found" });
            }

            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            var isInstructor = User.IsInRole("Instructor") || User.IsInRole("Admin");
            var isCourseInstructor = await _context.CourseInstructors
                .AnyAsync(ci => ci.CourseId == topic.Forum.CourseId && ci.UserName == userName);

            if (!isInstructor && !isCourseInstructor)
            {
                return Json(new { success = false, message = "Bạn không có quyền thực hiện hành động này." });
            }

            topic.Status = topic.Status == "Locked" ? "New" : "Locked";
            _topicRepository.Update(topic);
            _topicRepository.Save();

            return Json(new { success = true, status = topic.Status });
        }

        // POST: Forum/DeletePost
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePost(string postId)
        {
            if (string.IsNullOrEmpty(postId))
            {
                return Json(new { success = false, message = "PostId is required" });
            }

            var post = _postRepository.GetById(postId);
            if (post == null)
            {
                return Json(new { success = false, message = "Post not found" });
            }

            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            var isInstructor = User.IsInRole("Instructor") || User.IsInRole("Admin");
            var isPostOwner = post.UserName == userName;
            var isCourseInstructor = false;

            if (post.Topic != null && post.Topic.Forum != null)
            {
                isCourseInstructor = await _context.CourseInstructors
                    .AnyAsync(ci => ci.CourseId == post.Topic.Forum.CourseId && ci.UserName == userName);
            }

            if (!isPostOwner && !isInstructor && !isCourseInstructor)
            {
                return Json(new { success = false, message = "Bạn không có quyền xóa bài viết này." });
            }

            _postRepository.Delete(postId);
            _postRepository.Save();

            // Cập nhật reply count của topic
            if (post.Topic != null)
            {
                var topic = _topicRepository.GetById(post.TopicId);
                if (topic != null)
                {
                    topic.ReplyCount = Math.Max(0, topic.ReplyCount - 1);
                    _topicRepository.Update(topic);
                    _topicRepository.Save();
                }
            }

            return Json(new { success = true, message = "Đã xóa bài viết." });
        }

        // POST: Forum/DeleteTopic
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTopic(string topicId)
        {
            if (string.IsNullOrEmpty(topicId))
            {
                return Json(new { success = false, message = "TopicId is required" });
            }

            var topic = _topicRepository.GetById(topicId);
            if (topic == null)
            {
                return Json(new { success = false, message = "Topic not found" });
            }

            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            var isInstructor = User.IsInRole("Instructor") || User.IsInRole("Admin");
            var isTopicOwner = topic.UserName == userName;
            var isCourseInstructor = false;

            if (topic.Forum != null)
            {
                isCourseInstructor = await _context.CourseInstructors
                    .AnyAsync(ci => ci.CourseId == topic.Forum.CourseId && ci.UserName == userName);
            }

            if (!isTopicOwner && !isInstructor && !isCourseInstructor)
            {
                return Json(new { success = false, message = "Bạn không có quyền xóa chủ đề này." });
            }

            _topicRepository.Delete(topicId);
            _topicRepository.Save();

            return Json(new { success = true, message = "Đã xóa chủ đề." });
        }

        // POST: Forum/UploadImage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadImage(IFormFile image)
        {
            if (image == null || image.Length == 0)
            {
                return Json(new { success = false, message = "Không có file được chọn." });
            }

            if (!image.ContentType.StartsWith("image/"))
            {
                return Json(new { success = false, message = "File phải là hình ảnh." });
            }

            try
            {
                var uploadDirectory = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "forum");
                if (!Directory.Exists(uploadDirectory))
                {
                    Directory.CreateDirectory(uploadDirectory);
                }

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                var filePath = Path.Combine(uploadDirectory, fileName);
                var relativePath = $"/uploads/forum/{fileName}";

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                return Json(new { success = true, url = relativePath });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image");
                return Json(new { success = false, message = "Lỗi khi tải hình ảnh lên." });
            }
        }

        private async Task LoadRepliesRecursive(Post post)
        {
            var replies = await _postRepository.GetRepliesByPostId(post.PostId)
                .OrderBy(r => r.CreatedDate)
                .ToListAsync();

            foreach (var reply in replies)
            {
                reply.ParentPost = post;
                await LoadRepliesRecursive(reply);
            }

            post.Replies = replies;
        }
    }

}

