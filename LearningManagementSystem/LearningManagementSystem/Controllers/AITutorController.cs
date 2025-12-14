using LearningManagementSystem.Data;
using LearningManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting; // Thêm
using Newtonsoft.Json;
using System.Security.Claims;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace LearningManagementSystem.Controllers
{
    [Authorize(Roles = "Student")]
    [ValidateAntiForgeryToken]
    public class AITutorController : Controller
    {
        private readonly IGroqService _groqService;
        private readonly LMSContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment; // Thêm

        public AITutorController(IGroqService groqService, LMSContext context, IWebHostEnvironment webHostEnvironment)
        {
            _groqService = groqService;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public class AskRequest
        {
            public string CourseId { get; set; }
            public string Question { get; set; }
            public string? LessonContext { get; set; }
        }

        public class GenerateRequest
        {
            public string CourseId { get; set; }
            public string? Topic { get; set; }
            public string? LessonContext { get; set; }
            public string? LessonId { get; set; }
            public int? Count { get; set; }
            public string? Difficulty { get; set; }
        }

        public class SubmitAnswerRequest
        {
            public string PracticeId { get; set; }
            public string UserAnswer { get; set; }
        }

        public class TranslateRequest
        {
            public string CourseId { get; set; } = null!;
            public string? LessonId { get; set; }
            public string TargetLanguage { get; set; } = null!;
            public string? Text { get; set; }
        }

        private static readonly Dictionary<string, string> SupportedLanguages = new(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = "English",
            ["ja"] = "Japanese",
            ["ko"] = "Korean",
            ["fr"] = "French",
            ["es"] = "Spanish",
            ["zh"] = "Chinese (Simplified)",
            ["vi"] = "Vietnamese"
        };

        [HttpPost]
        public async Task<IActionResult> Ask([FromBody] AskRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.CourseId) || string.IsNullOrWhiteSpace(request?.Question))
            {
                return BadRequest("Thiếu CourseId hoặc câu hỏi.");
            }

            var course = await _context.Courses.AsNoTracking().FirstOrDefaultAsync(c => c.CourseId == request.CourseId);
            if (course == null) return NotFound("Không tìm thấy khóa học.");

            var messages = new List<ChatMessage>
            {
                new ChatMessage { Role = "system", Content = "Bạn là AI Tutor hỗ trợ hỏi đáp cho học viên theo nội dung bài học, trả lời ngắn gọn, rõ ràng." },
                new ChatMessage { Role = "user", Content = $"Khóa học: {course.CourseName}. Ngữ cảnh bài học (nếu có): {request.LessonContext ?? ""}" },
                new ChatMessage { Role = "user", Content = request.Question }
            };

            var answer = await _groqService.GetChatResponseAsync(messages);
            return Ok(new { answer });
        }

        [HttpPost]
        public async Task<IActionResult> GenerateQuestion([FromBody] GenerateRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.CourseId))
            {
                return BadRequest("Thiếu CourseId.");
            }

            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userName)) return Unauthorized();

            var course = await _context.Courses.AsNoTracking().FirstOrDefaultAsync(c => c.CourseId == request.CourseId);
            if (course == null) return NotFound("Không tìm thấy khóa học.");

            var effectiveContext = await BuildLessonContextAsync(request.LessonId, request.LessonContext);
            var difficulty = string.IsNullOrWhiteSpace(request.Difficulty) ? "trung bình" : request.Difficulty;
            var contextText = $"Khóa học: {course.CourseName}. Chủ đề: {request.Topic ?? "ngẫu nhiên theo bài học"}. Mức độ: {difficulty}. Ngữ cảnh: {effectiveContext}";

            var prompt = @"Hãy tạo MỘT câu hỏi trắc nghiệm phù hợp ngữ cảnh dưới đây.
Trả về JSON đúng định dạng:
{
  ""question"": ""string"",
  ""options"": [""A. ..."", ""B. ..."", ""C. ..."", ""D. ...""],
  ""answer"": ""A|B|C|D""
}";

            var messages = new List<ChatMessage>
            {
                new ChatMessage { Role = "system", Content = prompt },
                new ChatMessage { Role = "user", Content = contextText }
            };

            var raw = await _groqService.GetChatResponseAsync(messages);
            var jsonText = TryExtractJson(raw);
            if (string.IsNullOrEmpty(jsonText)) return BadRequest("AI không trả về JSON hợp lệ.");

            dynamic obj;
            try { obj = JsonConvert.DeserializeObject(jsonText); }
            catch { return BadRequest("Không phân tích được JSON."); }

            string question = obj.question;
            var optionsArr = ((IEnumerable<object>)obj.options).Select(o => o.ToString()).ToList();
            string correctRaw = obj.answer;
            string correct = System.Text.RegularExpressions.Regex.Match((correctRaw ?? string.Empty).ToString(), "[A-Da-d]").Value.ToUpper();

            if (string.IsNullOrWhiteSpace(question) || optionsArr.Count < 2 || string.IsNullOrWhiteSpace(correct))
            {
                return BadRequest("Thiếu dữ liệu câu hỏi.");
            }

            var practice = new AIPractice
            {
                UserName = userName,
                CourseId = request.CourseId,
                Question = question,
                Options = JsonConvert.SerializeObject(optionsArr),
                CorrectAnswer = correct
            };

            _context.AIPractices.Add(practice);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                practiceId = practice.Id,
                question,
                options = optionsArr,
                correctAnswer = correct
            });
        }

        [HttpPost]
        public async Task<IActionResult> GenerateQuestions([FromBody] GenerateRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.CourseId) || request.Count is null || request.Count < 1 || request.Count > 10)
            {
                return BadRequest("Thiếu CourseId hoặc Count không hợp lệ (1-10).");
            }

            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userName)) return Unauthorized();

            var course = await _context.Courses.AsNoTracking().FirstOrDefaultAsync(c => c.CourseId == request.CourseId);
            if (course == null) return NotFound("Không tìm thấy khóa học.");

            var effectiveContext = await BuildLessonContextAsync(request.LessonId, request.LessonContext);
            var difficulty = string.IsNullOrWhiteSpace(request.Difficulty) ? "trung bình" : request.Difficulty;
            var systemPrompt = @"Tạo N câu hỏi trắc nghiệm theo ngữ cảnh cung cấp.
Trả về JSON mảng 'items' gồm các phần tử có dạng:
{ ""question"": string, ""options"": [""A. ..."", ""B. ..."", ""C. ..."", ""D. ...""], ""answer"": ""A|B|C|D"" }
KHÔNG thêm trường khác.";

            var messages = new List<ChatMessage>
            {
                new ChatMessage { Role = "system", Content = systemPrompt },
                new ChatMessage { Role = "user", Content = $"Số lượng: {request.Count}. Chủ đề: {request.Topic ?? "Theo nội dung"}. Mức độ: {difficulty}. Ngữ cảnh: {effectiveContext}" }
            };

            var raw = await _groqService.GetChatResponseAsync(messages);
            var jsonText = TryExtractJson(raw);
            if (string.IsNullOrEmpty(jsonText)) return BadRequest("AI không trả về JSON hợp lệ.");

            dynamic obj;
            try { obj = JsonConvert.DeserializeObject(jsonText); }
            catch { return BadRequest("Không phân tích được JSON."); }

            var items = new List<object>();
            foreach (var it in obj.items)
            {
                string question = it.question;
                var optionsArr = ((IEnumerable<object>)it.options).Select(o => o.ToString()).ToList();
                string correct = System.Text.RegularExpressions.Regex.Match((it.answer ?? string.Empty).ToString(), "[A-Da-d]").Value.ToUpper();

                if (string.IsNullOrWhiteSpace(question) || optionsArr.Count < 2 || string.IsNullOrWhiteSpace(correct)) continue;

                var practice = new AIPractice
                {
                    UserName = userName,
                    CourseId = request.CourseId,
                    Question = question,
                    Options = JsonConvert.SerializeObject(optionsArr),
                    CorrectAnswer = correct
                };

                _context.AIPractices.Add(practice);
                items.Add(new { practiceId = practice.Id, question, options = optionsArr });
            }

            await _context.SaveChangesAsync();
            return Ok(new { items });
        }

        [HttpGet]
        public async Task<IActionResult> GetStats(string courseId)
        {
            if (string.IsNullOrWhiteSpace(courseId))
            {
                return BadRequest("Thiếu CourseId.");
            }

            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userName)) return Unauthorized();

            var practices = await _context.AIPractices
                .AsNoTracking()
                .Where(ap => ap.UserName == userName && ap.CourseId == courseId)
                .ToListAsync();

            if (practices.Count == 0)
            {
                return Ok(new { total = 0, correct = 0, accuracy = 0.0 });
            }

            var total = practices.Count;
            var correct = practices.Count(ap => ap.IsCorrect == true);
            var accuracy = total > 0 ? Math.Round(correct * 100.0 / total, 1) : 0.0;

            return Ok(new { total, correct, accuracy });
        }

        [HttpPost]
        public async Task<IActionResult> SummarizeLesson([FromBody] GenerateRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.CourseId) || string.IsNullOrWhiteSpace(request?.LessonId))
            {
                return BadRequest("Thiếu CourseId hoặc LessonId.");
            }

            var lesson = await _context.Lessons.AsNoTracking().FirstOrDefaultAsync(l => l.LessonId == request.LessonId);
            if (lesson == null) return NotFound("Không tìm thấy bài học.");

            var contextText = await BuildLessonContextAsync(request.LessonId, request.LessonContext);
            var messages = new List<ChatMessage>
            {
                new ChatMessage { Role = "system", Content = "Tóm tắt ngắn gọn, có gạch đầu dòng, tập trung ý chính." },
                new ChatMessage { Role = "user", Content = contextText }
            };

            var summary = await _groqService.GetChatResponseAsync(messages);
            return Ok(new { summary });
        }

        private async Task<string> BuildLessonContextAsync(string? lessonId, string? fallback)
        {
            if (string.IsNullOrEmpty(lessonId)) return fallback ?? string.Empty;

            var lesson = await _context.Lessons.AsNoTracking().FirstOrDefaultAsync(l => l.LessonId == lessonId);
            if (lesson == null) return fallback ?? string.Empty;

            // Ưu tiên lấy Content từ database Lesson, không lấy transcript video
            if (!string.IsNullOrWhiteSpace(lesson.Content))
            {
                return lesson.Content;
            }

            // Nếu không có Content, mới dùng fallback (nội dung từ frontend)
            return fallback ?? string.Empty;
        }

        private string? TryLoadTranscriptFromVideoUrl(string? videoUrl)
        {
            if (string.IsNullOrWhiteSpace(videoUrl)) return null;

            try
            {
                var fileName = Path.GetFileNameWithoutExtension(videoUrl);
                var dir = Path.GetDirectoryName(videoUrl)?.Replace("/", Path.DirectorySeparatorChar.ToString()) ?? string.Empty;
                var basePath = Path.Combine(_webHostEnvironment.WebRootPath, dir.TrimStart(Path.DirectorySeparatorChar));

                string Combine(string ext) => Path.Combine(basePath, fileName + ext);

                var vtt = Combine(".vtt");
                var srt = Combine(".srt");

                if (System.IO.File.Exists(vtt)) return ParseVtt(System.IO.File.ReadAllText(vtt));
                if (System.IO.File.Exists(srt)) return ParseSrt(System.IO.File.ReadAllText(srt));
            }
            catch { }

            return null;
        }

        private static string ParseVtt(string content)
        {
            var lines = content.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            var sb = new StringBuilder();
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (line.StartsWith("WEBVTT", StringComparison.OrdinalIgnoreCase)) continue;
                if (line.Contains("-->")) continue;
                sb.AppendLine(line.Trim());
            }
            return sb.ToString();
        }

        private static string ParseSrt(string content)
        {
            var lines = content.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            var sb = new StringBuilder();
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (int.TryParse(line.Trim(), out _)) continue;
                if (line.Contains("-->")) continue;
                sb.AppendLine(line.Trim());
            }
            return sb.ToString();
        }

        [HttpPost]
        public async Task<IActionResult> SubmitAnswer([FromBody] SubmitAnswerRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.PracticeId) || string.IsNullOrWhiteSpace(request?.UserAnswer))
            {
                return BadRequest("Thiếu PracticeId hoặc đáp án.");
            }

            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userName)) return Unauthorized();

            var practice = await _context.AIPractices
                .FirstOrDefaultAsync(p => p.Id == request.PracticeId && p.UserName == userName);

            if (practice == null) return NotFound("Không tìm thấy bài luyện.");

            practice.UserAnswer = request.UserAnswer.Trim().ToUpper();
            practice.IsCorrect = string.Equals(practice.UserAnswer, practice.CorrectAnswer, StringComparison.OrdinalIgnoreCase);
            practice.AnsweredAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { isCorrect = practice.IsCorrect, correctAnswer = practice.CorrectAnswer });
        }

        [HttpPost]
        public async Task<IActionResult> TranslateLesson([FromBody] TranslateRequest request)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.CourseId) ||
                string.IsNullOrWhiteSpace(request.TargetLanguage))
            {
                return BadRequest("Thiếu CourseId hoặc ngôn ngữ cần dịch.");
            }

            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userName))
            {
                return Unauthorized();
            }

            if (!SupportedLanguages.TryGetValue(request.TargetLanguage.Trim().ToLowerInvariant(), out var languageName))
            {
                return BadRequest("Ngôn ngữ chưa được hỗ trợ.");
            }

            string sourceText = request.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(sourceText) && !string.IsNullOrWhiteSpace(request.LessonId))
            {
                var lesson = await _context.Lessons
                    .AsNoTracking()
                    .FirstOrDefaultAsync(l => l.LessonId == request.LessonId && l.CourseId == request.CourseId);

                if (lesson != null && !string.IsNullOrWhiteSpace(lesson.Content))
                {
                    sourceText = lesson.Content;
                }
            }

            if (string.IsNullOrWhiteSpace(sourceText))
            {
                return BadRequest("Không có nội dung để dịch.");
            }

            // Làm sạch text - loại bỏ HTML tags nếu có, nhưng giữ nguyên format
            string cleanText = System.Text.RegularExpressions.Regex.Replace(sourceText, "<.*?>", string.Empty);
            cleanText = cleanText.Trim();

            // Tạo prompt rõ ràng và trực tiếp - chỉ dịch, không trả lời hay giải thích
            // Đặc biệt nhấn mạnh với tiếng Anh vì AI thường có xu hướng trả lời câu hỏi bằng tiếng Anh
            var systemPrompt = languageName.Equals("English", StringComparison.OrdinalIgnoreCase)
                ? "You are a translation tool ONLY. Translate the Vietnamese text to English. Translate EXACTLY what is written. If the input is a question like 'c++ là gì', translate it to 'What is C++' - DO NOT answer the question, DO NOT provide explanations, DO NOT add any information. Output ONLY the direct translation, nothing else."
                : $"You are a translation tool. Translate the Vietnamese text to {languageName}. Translate word-for-word or sentence-by-sentence. Do NOT answer questions, do NOT provide explanations, do NOT add any information. If the input is a question, translate only the question itself. Output ONLY the translation in {languageName}, nothing else.";
            
            var messages = new List<ChatMessage>
            {
                new ChatMessage
                {
                    Role = "system",
                    Content = systemPrompt
                },
                new ChatMessage
                {
                    Role = "user",
                    Content = cleanText
                }
            };

            var translation = await _groqService.GetChatResponseAsync(messages);
            
            // Làm sạch kết quả - loại bỏ các phần không cần thiết
            if (!string.IsNullOrWhiteSpace(translation))
            {
                translation = translation.Trim();
                
                // Loại bỏ các prefix thường gặp
                translation = System.Text.RegularExpressions.Regex.Replace(
                    translation, 
                    @"^(Translation|Bản dịch|Dịch|Translated|Here is the translation|Here's the translation|The translation is|Bản dịch là)[:\s]*", 
                    "", 
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Multiline
                );
                
                // Loại bỏ các pattern như "Vietnamese: ..." hoặc "English: ..."
                translation = System.Text.RegularExpressions.Regex.Replace(
                    translation,
                    @"^(Vietnamese|English|Japanese|Korean|French|Spanish|Chinese|Tiếng Việt|Tiếng Anh)[:\s]+",
                    "",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Multiline
                );
                
                // Nếu văn bản gốc là câu hỏi ngắn (dưới 50 ký tự), chỉ lấy phần đầu tiên (câu hỏi đã dịch)
                // để tránh lấy phần giải thích dài - đặc biệt quan trọng với tiếng Anh
                if (cleanText.Length < 50 && translation.Length > cleanText.Length * 2)
                {
                    // Tách theo dấu chấm, xuống dòng, dấu hai chấm, hoặc dấu phẩy (nếu quá dài)
                    var sentences = System.Text.RegularExpressions.Regex.Split(translation, @"[\.\n:]");
                    var firstSentence = sentences[0].Trim();
                    
                    // Nếu câu đầu tiên vẫn quá dài, thử tách theo dấu phẩy
                    if (firstSentence.Length > cleanText.Length * 3 && firstSentence.Contains(','))
                    {
                        firstSentence = firstSentence.Split(',')[0].Trim();
                    }
                    
                    if (!string.IsNullOrWhiteSpace(firstSentence) && firstSentence.Length <= cleanText.Length * 3)
                    {
                        translation = firstSentence;
                    }
                    // Nếu vẫn không hợp lý, thử lấy phần ngắn nhất từ các câu
                    else if (sentences.Length > 1)
                    {
                        var shortest = sentences
                            .Where(s => !string.IsNullOrWhiteSpace(s) && s.Trim().Length <= cleanText.Length * 3)
                            .OrderBy(s => s.Trim().Length)
                            .FirstOrDefault();
                        if (!string.IsNullOrWhiteSpace(shortest))
                        {
                            translation = shortest.Trim();
                        }
                    }
                }
                
                translation = translation.Trim();
            }
            
            return Ok(new
            {
                translation,
                targetLanguage = languageName
            });
        }

        private static string? TryExtractJson(string input)
        {
            if (string.IsNullOrEmpty(input)) return null;

            var start = input.IndexOf('{');
            var end = input.LastIndexOf('}');
            if (start >= 0 && end > start)
            {
                return input.Substring(start, end - start + 1);
            }
            return null;
        }
    }
}