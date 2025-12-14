using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
                var originalInput = request.UserInput ?? string.Empty;
                var userInput = originalInput.ToLowerInvariant();

                // Người dùng chỉ quan tâm đến giá/miễn phí
                if (TryGetPriceOnlyFilter(originalInput, out var priceOnlyFilter))
                {
                    var priceCourses = await FetchCoursesByPriceFilterAsync(priceOnlyFilter);
                    if (priceCourses.Any())
                    {
                        var cards = string.Join("", priceCourses.Select(BuildCourseCardHtml));
                        var summary = BuildFilterSummary(null, priceOnlyFilter);
                        var reply = $"<div>Mình gợi ý vài khóa học phù hợp{summary}:</div>{cards}";
                        messages.Add(new ChatMessage { Role = "assistant", Content = reply });
                    }
                    else
                    {
                        messages.Add(new ChatMessage
                        {
                            Role = "assistant",
                            Content = "Mình chưa thấy khóa học nào thỏa điều kiện giá bạn đưa ra. Bạn có thể điều chỉnh mức giá hoặc cung cấp thêm chủ đề để mình tìm giúp nhé!"
                        });
                    }
                    return Json(new { success = true, messages });
                }

                // Người dùng yêu cầu cụ thể "khóa <tên>"
                if (TryGetRequestedCourseName(originalInput, out var requestedCourseName))
                {
                    var exactCourse = await FindCourseByNameAsync(requestedCourseName);
                    if (exactCourse != null)
                    {
                        var reply = $"<div>Mình tìm thấy khóa học \"{exactCourse.CourseName}\" bạn hỏi:</div>{BuildCourseCardHtml(exactCourse)}";
                        messages.Add(new ChatMessage { Role = "assistant", Content = reply });
                    }
                    else
                    {
                        messages.Add(new ChatMessage { Role = "assistant", Content = "Hiện tại chưa tìm thấy khóa học đúng tên bạn yêu cầu. Bạn có thể kiểm tra lại tên khóa học hoặc mô tả rõ hơn nhé!" });
                    }
                    return Json(new { success = true, messages });
                }

                // Nếu user hỏi gợi ý khóa học thì trả về danh sách khóa học thực tế
                if (IsRecommendationIntent(userInput))
                {
                    var recommendation = await FetchRecommendedCoursesAsync(userInput);
                    if (recommendation.Courses.Count == 0)
                    {
                        messages.Add(new ChatMessage { Role = "assistant", Content = "Hiện tại chưa tìm thấy khóa học phù hợp. Bạn có thể thử mô tả cụ thể hơn về mục tiêu hoặc ngôn ngữ muốn học nhé!" });
                    }
                    else
                    {
                        var courseList = string.Join("", recommendation.Courses.Select(BuildCourseCardHtml));
                        var filterSummary = BuildFilterSummary(recommendation.DetectedLevel, recommendation.PriceFilter);
                        var reply = $"<div>Mình gợi ý vài khóa học phù hợp{filterSummary}:</div>{courseList}<div>Bạn có thể nhấp vào tên khóa học để xem chi tiết hoặc mô tả rõ hơn nhu cầu để mình gợi ý chính xác hơn.</div>";
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
        private bool IsRecommendationIntent(string input)
        {
            var normalized = (input ?? string.Empty).Normalize(NormalizationForm.FormC).ToLowerInvariant();
            var normalizedPlain = RemoveDiacritics(normalized);

            return ContainsAny(normalized, normalizedPlain, "gợi ý", "goi y", "khóa học", "khoa hoc",
                "course", "tìm khóa học", "tim khoa hoc", "nên học gì", "nen hoc gi", "học gì", "hoc gi",
                "giá", "gia", "miễn phí", "mien phi", "free", "khóa", "khoa", "học phí", "hoc phi");
        }

        private async Task<RecommendationResult> FetchRecommendedCoursesAsync(string userInput)
        {
            var normalized = (userInput ?? string.Empty).Normalize(NormalizationForm.FormC).ToLowerInvariant();
            var normalizedPlain = RemoveDiacritics(normalized);
            var tags = await _context.Tags.Where(t => t.IsActive).ToListAsync();
            var matchedTagIds = tags
                .Where(tag =>
                    ContainsText(normalized, normalizedPlain, tag.Name) ||
                    ContainsText(normalized, normalizedPlain, tag.Slug?.Replace("-", " ")))
                .Select(tag => tag.TagId)
                .Distinct()
                .ToList();

            var detectedLevel = DetectLevel(normalized, normalizedPlain);
            var priceFilter = DetectPriceFilter(normalized, normalizedPlain);
            var keywords = ExtractKeywords(normalized);
            var condensedQuery = string.Join(" ", keywords);

            var baseQuery = _context.Courses
                .Include(c => c.CourseTags)
                    .ThenInclude(ct => ct.Tag)
                .OrderBy(c => c.CourseName)
                .AsQueryable();

            if (matchedTagIds.Any())
            {
                baseQuery = baseQuery.Where(c => c.CourseTags.Any(ct => matchedTagIds.Contains(ct.TagId)));
            }

            var filteredQuery = ApplyLevelFilter(baseQuery, detectedLevel);
            filteredQuery = ApplyPriceFilter(filteredQuery, priceFilter);

            var candidateCourses = await filteredQuery.Take(30).ToListAsync();
            var effectiveLevel = detectedLevel;
            var effectivePrice = priceFilter;
            var relaxedFilters = false;

            if (!candidateCourses.Any() && !string.IsNullOrEmpty(detectedLevel))
            {
                filteredQuery = ApplyPriceFilter(baseQuery, priceFilter);
                candidateCourses = await filteredQuery.Take(30).ToListAsync();
                effectiveLevel = null;
                relaxedFilters = true;
            }

            if (!candidateCourses.Any() && priceFilter != null)
            {
                filteredQuery = ApplyLevelFilter(baseQuery, effectiveLevel);
                candidateCourses = await filteredQuery.Take(30).ToListAsync();
                effectivePrice = null;
                relaxedFilters = true;
            }

            if (!candidateCourses.Any())
            {
                candidateCourses = await baseQuery.Take(30).ToListAsync();
            }

            if (!string.IsNullOrWhiteSpace(condensedQuery))
            {
                var phraseMatches = candidateCourses
                    .Where(course => ContainsNormalized(course.CourseName, condensedQuery) ||
                                     ContainsNormalized(course.Description, condensedQuery))
                    .ToList();

                if (phraseMatches.Any())
                {
                    candidateCourses = phraseMatches;
                }
            }

            if (keywords.Any())
            {
                var keywordMatches = candidateCourses
                    .Where(course => MatchesKeywords(course, keywords))
                    .ToList();

                if (!keywordMatches.Any())
                {
                    var fallbackPool = await baseQuery.Take(200).ToListAsync();
                    keywordMatches = fallbackPool
                        .Where(course => MatchesKeywords(course, keywords))
                        .ToList();

                    if (keywordMatches.Any())
                    {
                        candidateCourses = keywordMatches;
                        effectiveLevel = null;
                        effectivePrice = null;
                        relaxedFilters = true;
                    }
                    else
                    {
                        candidateCourses = new List<Course>();
                        effectiveLevel = null;
                        effectivePrice = null;
                        relaxedFilters = false;
                    }
                }
                else
                {
                    candidateCourses = keywordMatches;
                }
            }

            var result = new RecommendationResult
            {
                Courses = candidateCourses.Take(5).ToList(),
                DetectedLevel = effectiveLevel,
                PriceFilter = effectivePrice,
                WasRelaxed = relaxedFilters
            };

            return result;
        }

        private static string? DetectLevel(string normalized, string normalizedPlain)
        {
            if (ContainsAny(normalized, normalizedPlain, "beginner", "mới bắt đầu", "moi bat dau"))
            {
                return "Beginner";
            }
            if (ContainsAny(normalized, normalizedPlain, "advanced", "nâng cao", "nang cao"))
            {
                return "Advanced";
            }
            if (ContainsAny(normalized, normalizedPlain, "intermediate", "trung cấp", "trung cap", "trung bình", "trung binh"))
            {
                return "Intermediate";
            }

            return null;
        }

        private static PriceFilter? DetectPriceFilter(string normalized, string normalizedPlain)
        {
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return null;
            }

            var filter = new PriceFilter();

            if (ContainsAny(normalized, normalizedPlain, "miễn phí", "mien phi", "free"))
            {
                filter.OnlyFree = true;
                return filter;
            }

            ApplySymbolBasedFilter(normalized, filter);

            filter.MaxPrice ??= FindAmountByKeywords(normalized, new[]
            {
                "bé hơn", "be hon", "ít hơn", "it hon", "dưới", "duoi", "tối đa", "toi da", "max", "nhỏ hơn", "nho hon", "không quá", "khong qua", "ko quá", "ko qua"
            });

            filter.MinPrice ??= FindAmountByKeywords(normalized, new[]
            {
                "trên", "tren", "lớn hơn", "lon hon", "cao hơn", "cao hon", "tối thiểu", "toi thieu", "ít nhất", "it nhat", "từ", "tu", "min"
            });

            if (filter.MaxPrice == null)
            {
                filter.MaxPrice = FindAmountBeforeKeywords(normalized, new[]
                {
                    "trở xuống", "tro xuong", "hoặc ít hơn", "hoac it hon", "hoặc thấp hơn", "hoac thap hon"
                });
            }

            if (filter.MinPrice == null)
            {
                filter.MinPrice = FindAmountBeforeKeywords(normalized, new[]
                {
                    "trở lên", "tro len", "hoặc cao hơn", "hoac cao hon"
                });
            }

            if (!filter.HasConstraint)
            {
                var looseAmount = FindFirstAmount(normalized);
                if (looseAmount.HasValue)
                {
                    filter.MaxPrice = looseAmount;
                }
            }

            if (filter.MinPrice.HasValue && filter.MaxPrice.HasValue && filter.MinPrice > filter.MaxPrice)
            {
                filter.MinPrice = null;
            }

            return filter.HasConstraint ? filter : null;
        }

        private static void ApplySymbolBasedFilter(string normalized, PriceFilter filter)
        {
            var symbolPattern = new Regex(@"(?<symbol>>=|<=|>|<)\s*(?<amount>[\d.,]+\s*(?:k|nghìn|ngàn|ngan|triệu|trieu|tr|m|tỉ|ty|tỷ)?)",
                RegexOptions.Compiled);

            foreach (Match match in symbolPattern.Matches(normalized))
            {
                var amount = ParseAmount(match.Groups["amount"].Value);
                if (!amount.HasValue)
                {
                    continue;
                }

                var symbol = match.Groups["symbol"].Value;
                switch (symbol)
                {
                    case ">" or ">=":
                        filter.MinPrice = amount;
                        break;
                    case "<" or "<=":
                        filter.MaxPrice = amount;
                        break;
                }
            }
        }

        private static decimal? FindAmountByKeywords(string normalized, string[] keywords)
        {
            foreach (var keyword in keywords)
            {
                var index = normalized.IndexOf(keyword, System.StringComparison.Ordinal);
                if (index < 0)
                {
                    continue;
                }

                var slice = normalized[(index + keyword.Length)..];
                var amount = ParseAmount(slice);
                if (amount.HasValue)
                {
                    return amount;
                }
            }

            return null;
        }

        private static decimal? FindAmountBeforeKeywords(string normalized, string[] keywords)
        {
            foreach (var keyword in keywords)
            {
                var index = normalized.IndexOf(keyword, System.StringComparison.Ordinal);
                if (index <= 0)
                {
                    continue;
                }

                var slice = normalized[..index];
                var amount = ParseAmount(slice);
                if (amount.HasValue)
                {
                    return amount;
                }
            }

            return null;
        }

        private static decimal? ParseAmount(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return null;
            }

            var cleaned = raw
                .Replace("vnđ", "")
                .Replace("vnd", "")
                .Replace("đồng", "")
                .Replace("đ", "")
                .Trim();

            var pattern = new Regex(@"(?<value>\d+(?:[.,]\d+)?)(?<unit>k|nghìn|ngan|ngàn|triệu|trieu|tr|m|tỉ|ty|tỷ)?",
                RegexOptions.Compiled);
            var match = pattern.Match(cleaned);
            if (!match.Success)
            {
                return null;
            }

            var numberPart = match.Groups["value"].Value
                .Replace(".", "")
                .Replace(",", ".");

            if (!decimal.TryParse(numberPart, NumberStyles.Number, CultureInfo.InvariantCulture, out var number))
            {
                return null;
            }

            var unit = match.Groups["unit"].Value.ToLowerInvariant();
            var multiplier = unit switch
            {
                "k" => 1_000m,
                "nghìn" or "ngan" or "ngàn" => 1_000m,
                "triệu" or "trieu" or "tr" or "m" => 1_000_000m,
                "tỉ" or "ty" or "tỷ" => 1_000_000_000m,
                _ => 1m
            };

            return number * multiplier;
        }

        private static decimal? FindFirstAmount(string normalized)
        {
            var pattern = new Regex(@"(\d+(?:[.,]\d+)?\s*(k|nghìn|ngan|ngàn|triệu|trieu|tr|m|tỉ|ty|tỷ)?)",
                RegexOptions.Compiled);
            var match = pattern.Match(normalized);
            if (!match.Success)
            {
                return null;
            }

            return ParseAmount(match.Value);
        }

        private static string BuildFilterSummary(string? level, PriceFilter? filter)
        {
            var parts = new List<string>();

            if (!string.IsNullOrEmpty(level))
            {
                parts.Add(level switch
                {
                    "Beginner" => "trình độ Beginner",
                    "Intermediate" => "trình độ Intermediate",
                    "Advanced" => "trình độ Advanced",
                    _ => $"trình độ {level}"
                });
            }

            if (filter == null)
            {
                return parts.Any() ? $" ({string.Join(", ", parts)})" : string.Empty;
            }

            if (filter.OnlyFree)
            {
                parts.Add("miễn phí");
            }
            else
            {
                if (filter.MinPrice.HasValue && filter.MaxPrice.HasValue)
                {
                    parts.Add($"giá từ {FormatCurrency(filter.MinPrice.Value)} đến {FormatCurrency(filter.MaxPrice.Value)}");
                }
                else if (filter.MinPrice.HasValue)
                {
                    parts.Add($"giá từ {FormatCurrency(filter.MinPrice.Value)}");
                }
                else if (filter.MaxPrice.HasValue)
                {
                    parts.Add($"giá tối đa {FormatCurrency(filter.MaxPrice.Value)}");
                }
            }

            return parts.Any() ? $" (lọc theo {string.Join(", ", parts)})" : string.Empty;
        }

        private static string FormatCurrency(decimal amount)
        {
            return string.Format(CultureInfo.GetCultureInfo("vi-VN"), "{0:N0}đ", amount);
        }

        private sealed class PriceFilter
        {
            public decimal? MinPrice { get; set; }
            public decimal? MaxPrice { get; set; }
            public bool OnlyFree { get; set; }
            public bool HasConstraint => OnlyFree || MinPrice.HasValue || MaxPrice.HasValue;
        }

        private sealed class RecommendationResult
        {
            public List<Course> Courses { get; set; } = new();
            public string? DetectedLevel { get; set; }
            public PriceFilter? PriceFilter { get; set; }
            public bool WasRelaxed { get; set; }
        }

        private static readonly HashSet<string> KeywordNoise = new(StringComparer.OrdinalIgnoreCase)
        {
            "khóa", "hoc", "học", "khoa", "khóa học", "khoa hoc",
            "miễn", "mien", "phí", "phi", "free",
            "giúp", "giup", "mình", "minh", "tìm", "tim",
            "có", "co", "không", "khong", "ko",
            "cho", "xin", "vài", "vai", "nào", "nao", "được", "duoc",
            "bạn", "ban", "tôi", "toi", "với", "voi", "và", "va",
            "hay", "hoặc", "hoac", "cơ", "co", "bản", "ban", "cơ bản", "co ban",
            "beginner", "intermediate", "advanced", "mới", "moi", "bắt", "bat", "đầu", "dau",
            "level", "trình", "trinh", "độ", "do"
        };

        private static readonly string[] PriceQueryNoise =
        {
            "khoa", "khoa hoc", "khoahoc",
            "khoa", "khoá", "khoá học",
            "gia", "gia tien", "giatien", "gia ca",
            "mien", "mien phi", "mienphi", "phi",
            "free", "lon", "lon hon", "be", "be hon",
            "hon", "cao", "thap", "duoi", "tren",
            "tu", "toi thieu", "toi da", "bao nhieu"
        };

        private static List<string> ExtractKeywords(string normalized)
        {
            var delimiter = new[] { ' ', ',', '.', '?', '!', ':', ';', '\r', '\n', '\t', '/', '\\' };

            return normalized.Split(delimiter, System.StringSplitOptions.RemoveEmptyEntries)
                .Select(token => token.Trim())
                .Select(token => token.Trim('-').Trim('_'))
                .Where(token => token.Length > 1)
                .Where(token =>
                {
                    var plain = RemoveDiacritics(token);
                    return !KeywordNoise.Contains(token) && !KeywordNoise.Contains(plain);
                })
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(5)
                .ToList();
        }

        private static IQueryable<Course> ApplyLevelFilter(IQueryable<Course> query, string? level)
        {
            if (string.IsNullOrEmpty(level))
            {
                return query;
            }

            return query.Where(c => c.Level == level);
        }

        private static IQueryable<Course> ApplyPriceFilter(IQueryable<Course> query, PriceFilter? filter)
        {
            if (filter == null)
            {
                return query;
            }

            if (filter.OnlyFree)
            {
                return query.Where(c => c.Price == null || c.Price <= 0);
            }

            if (filter.MinPrice.HasValue)
            {
                var min = filter.MinPrice.Value;
                query = query.Where(c => c.Price.HasValue && c.Price.Value >= min);
            }

            if (filter.MaxPrice.HasValue)
            {
                var max = filter.MaxPrice.Value;
                query = query.Where(c => (!c.Price.HasValue && max >= 0) || (c.Price.HasValue && c.Price.Value <= max));
            }

            return query;
        }

        private static bool MatchesKeywords(Course course, List<string> keywords)
        {
            foreach (var keyword in keywords)
            {
                if (ContainsNormalized(course.CourseName, keyword) ||
                    ContainsNormalized(course.Description, keyword) ||
                    course.CourseTags?.Any(ct => ct.Tag != null && ContainsNormalized(ct.Tag.Name, keyword)) == true)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool ContainsNormalized(string? source, string keyword)
        {
            if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(keyword))
            {
                return false;
            }

            var sourceLower = source.Normalize(NormalizationForm.FormC).ToLowerInvariant();
            var keywordLower = keyword.Normalize(NormalizationForm.FormC).ToLowerInvariant();

            if (sourceLower.Contains(keywordLower))
            {
                return true;
            }

            var sourcePlain = RemoveDiacritics(sourceLower);
            var keywordPlain = RemoveDiacritics(keywordLower);
            if (sourcePlain.Contains(keywordPlain))
            {
                return true;
            }

            var sourceCompact = NormalizeCompact(sourceLower);
            var keywordCompact = NormalizeCompact(keywordLower);
            return sourceCompact.Contains(keywordCompact);
        }

        private static string NormalizeCompact(string input)
        {
            var plain = RemoveDiacritics(input);
            var builder = new StringBuilder(plain.Length);
            foreach (var ch in plain)
            {
                if (char.IsLetterOrDigit(ch))
                {
                    builder.Append(ch);
                }
            }

            return builder.ToString();
        }

        private static bool ContainsText(string source, string sourcePlain, string? keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return false;
            }

            var keywordLower = keyword.Normalize(NormalizationForm.FormC).ToLowerInvariant();
            if (source.Contains(keywordLower))
            {
                return true;
            }

            var keywordPlain = RemoveDiacritics(keywordLower);
            return sourcePlain.Contains(keywordPlain);
        }

        private static bool ContainsAny(string source, string sourcePlain, params string[] keywords)
        {
            foreach (var keyword in keywords)
            {
                if (ContainsText(source, sourcePlain, keyword))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool TryGetRequestedCourseName(string input, out string courseName)
        {
            courseName = string.Empty;
            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }

            var trimmed = input.Trim();
            var lowered = trimmed.ToLowerInvariant();
            var prefixes = new[]
            {
                "khóa học",
                "khoá học",
                "khoa hoc",
                "khóa",
                "khoá",
                "khoa"
            };

            foreach (var prefix in prefixes)
            {
                if (lowered.StartsWith(prefix + " "))
                {
                    courseName = trimmed.Substring(prefix.Length).Trim();
                    if (LooksLikePriceQuery(courseName))
                    {
                        courseName = string.Empty;
                        return false;
                    }
                    return courseName.Length > 0;
                }

                if (lowered == prefix)
                {
                    return false;
                }
            }

            return false;
        }

        private static bool LooksLikePriceQuery(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            var lower = text.ToLowerInvariant();
            if (lower.Contains("miễn phí") || lower.Contains("mien phi") || lower.Contains("free"))
            {
                return true;
            }

            var numberPattern = new Regex(@"^[\s><=]*(\d+([\.,]\d+)?)(k|nghìn|ngan|ngàn|triệu|trieu|tr|m|tỉ|ty|tỷ)?\s*$",
                RegexOptions.IgnoreCase);

            if (numberPattern.IsMatch(lower))
            {
                return true;
            }

            return lower.Contains(">") || lower.Contains("<");
        }

        private bool TryGetPriceOnlyFilter(string input, out PriceFilter? filter)
        {
            filter = null;
            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }

            var normalized = input.Normalize(NormalizationForm.FormC).ToLowerInvariant();
            var normalizedPlain = RemoveDiacritics(normalized);

            if (!normalized.Contains("khóa") && !normalized.Contains("khoá") && !normalizedPlain.Contains("khoa"))
            {
                return false;
            }

            var detected = DetectPriceFilter(normalized, normalizedPlain);
            if (detected == null)
            {
                return false;
            }

            var cleaned = normalizedPlain;
            foreach (var word in PriceQueryNoise)
            {
                cleaned = cleaned.Replace(word, " ");
            }
            cleaned = Regex.Replace(cleaned, @"[\d\s><=.,]+", " ");
            cleaned = Regex.Replace(cleaned, @"\b(k|nghin|ngan|ngan|ngan|ngan|nghìn|ngàn|trieu|tr|m|ti|ty|ty|ty|vnd|vnđ|d|dong|đ|đong)\b",
                " ", RegexOptions.IgnoreCase);
            cleaned = Regex.Replace(cleaned, @"\s+", " ").Trim();

            if (!string.IsNullOrWhiteSpace(cleaned))
            {
                return false;
            }

            filter = detected;
            return true;
        }

        private async Task<Course?> FindCourseByNameAsync(string courseName)
        {
            var normalizedQuery = NormalizeCompact(courseName.Normalize(NormalizationForm.FormC).ToLowerInvariant());
            if (string.IsNullOrWhiteSpace(normalizedQuery))
            {
                return null;
            }

            var courses = await _context.Courses
                .Include(c => c.CourseTags)
                    .ThenInclude(ct => ct.Tag)
                .OrderBy(c => c.CourseName)
                .AsNoTracking()
                .ToListAsync();

            return courses.FirstOrDefault(course =>
            {
                var name = course.CourseName ?? string.Empty;
                var courseCompact = NormalizeCompact(name.Normalize(NormalizationForm.FormC).ToLowerInvariant());
                return courseCompact.Contains(normalizedQuery) || normalizedQuery.Contains(courseCompact);
            });
        }

        private async Task<List<Course>> FetchCoursesByPriceFilterAsync(PriceFilter filter)
        {
            var query = _context.Courses
                .Include(c => c.CourseTags)
                    .ThenInclude(ct => ct.Tag)
                .OrderBy(c => c.CourseName)
                .AsQueryable();

            query = ApplyPriceFilter(query, filter);

            return await query.Take(5).ToListAsync();
        }

        private static string BuildCourseCardHtml(Course c)
        {
            var tags = c.CourseTags?
                .Select(ct => ct.Tag != null ? ct.Tag.Name : string.Empty)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Take(3)
                .ToList() ?? new List<string>();

            var chips = new List<string>();
            if (!string.IsNullOrWhiteSpace(c.Level))
            {
                chips.Add($"<span style='background:#e0f2ff;padding:2px 8px;border-radius:12px;font-size:12px;margin-right:4px;'>{c.Level}</span>");
            }
            if (c.DurationMinutes.HasValue)
            {
                chips.Add($"<span style='background:#fef3c7;padding:2px 8px;border-radius:12px;font-size:12px;margin-right:4px;'>{c.DurationMinutes} phút</span>");
            }
            foreach (var tag in tags)
            {
                chips.Add($"<span style='background:#f3f4f6;padding:2px 8px;border-radius:12px;font-size:12px;margin-right:4px;'>{tag}</span>");
            }

            var imageHtml = string.IsNullOrEmpty(c.ImageUrl)
                ? string.Empty
                : $"<img src='{c.ImageUrl}' alt='img' style='width:48px;height:48px;object-fit:cover;border-radius:8px;margin-right:10px;' />";

            return $@"<div style='display:flex;align-items:flex-start;margin-bottom:12px;padding-bottom:8px;border-bottom:1px solid #f1f5f9;'>
                {imageHtml}
                <div>
                    <a href='/Course/Details/{c.CourseId}' style='font-weight:600;color:#0ea5e9;text-decoration:none;'>{c.CourseName}</a>
                    <div style='color:#475569;font-size:14px;margin:2px 0;'>{(c.Price.HasValue ? $"{c.Price.Value:N0}đ" : "Miễn phí")}</div>
                    {(chips.Any() ? $"<div style='margin:4px 0;'>{string.Join("", chips)}</div>" : "")}
                </div>
            </div>";
        }

        private static string RemoveDiacritics(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            var normalized = input.Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder();
            foreach (var ch in normalized)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    builder.Append(ch);
                }
            }

            return builder.ToString().Normalize(NormalizationForm.FormC);
        }
    }

    public class ChatRequest
    {
        public string UserInput { get; set; }
        public List<ChatMessage> Messages { get; set; }
    }
}
