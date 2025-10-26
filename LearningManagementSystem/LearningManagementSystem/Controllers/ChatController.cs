using LearningManagementSystem.Data;
using LearningManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearningManagementSystem.Controllers
{
    public class ChatController : Controller
    {
        private readonly IGroqService _groqService;
        private readonly LMSContext _context;

        public ChatController(IGroqService groqService, LMSContext context)
        {
            _groqService = groqService;
            _context = context;
        }

        public IActionResult Index()
        {
            var model = new ChatViewModel();
            // Thêm tin nhắn chào mừng
            model.Messages.Add(new ChatMessage
            {
                Role = "assistant",
                Content = "Xin chào! Tôi là chatbot tư vấn khóa học. Bạn cần tư vấn về khóa học nào ạ?"
            });
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
        {
            try
            {
                var messages = request.Messages ?? new List<ChatMessage>();
                var userInput = request.UserInput?.ToLower() ?? "";

                // Nếu user hỏi gợi ý khóa học thì trả về danh sách khóa học thực tế
                if (userInput.Contains("gợi ý") || userInput.Contains("khóa học") || userInput.Contains("course") || userInput.Contains("tìm khóa học"))
                {
                    var courses = await _context.Courses.OrderBy(c => c.CourseName).Take(5).ToListAsync();
                    if (courses.Count == 0)
                    {
                        messages.Add(new ChatMessage { Role = "assistant", Content = "Hiện tại chưa có khóa học nào trong hệ thống." });
                    }
                    else
                    {
                        var courseList = string.Join("<br>", courses.Select((c, i) =>
                            $@"<div style='display:flex;align-items:center;margin-bottom:8px;'>"
                            + (string.IsNullOrEmpty(c.ImageUrl) ? "" : $"<img src='{c.ImageUrl}' alt='img' style='width:40px;height:40px;object-fit:cover;border-radius:8px;margin-right:10px;' />")
                            + $"<a href='/Course/Details/{c.CourseId}' style='font-weight:bold;color:#1976d2;text-decoration:underline;'>{c.CourseName}</a>"
                            + $"<span style='margin-left:8px;color:#388e3c;font-size:0.95em;'>({(c.Price.HasValue ? c.Price.Value.ToString("N0") + "đ" : "Miễn phí")})</span>"
                            + "</div>"
                        ));
                        var reply = $"<div>Các khóa học hiện có:</div>{courseList}<div>Bạn muốn tìm hiểu thêm về khóa học nào?</div>";
                        messages.Add(new ChatMessage { Role = "assistant", Content = reply });
                    }
                    return Json(new { success = true, messages = messages });
                }

                // Thêm tin nhắn của user
                messages.Add(new ChatMessage { Role = "user", Content = request.UserInput });

                // Gọi Groq API
                var response = await _groqService.GetChatResponseAsync(messages);

                // Thêm phản hồi của bot
                messages.Add(new ChatMessage { Role = "assistant", Content = response });

                return Json(new { success = true, messages = messages });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }
    }

    public class ChatRequest
    {
        public string UserInput { get; set; }
        public List<ChatMessage> Messages { get; set; }
    }
}
