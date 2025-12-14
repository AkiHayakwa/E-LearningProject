using LearningManagementSystem.Models;
using Newtonsoft.Json;
using System.Linq;
using System.Text;

public interface IGroqService
{
    Task<string> GetChatResponseAsync(List<ChatMessage> messages);
}

public class GroqService : IGroqService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public GroqService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["GroqApi:ApiKey"];
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
    }

    public async Task<string> GetChatResponseAsync(List<ChatMessage> messages)
    {
        try
        {
            var allMessages = new List<ChatMessage>();
            if (messages == null || messages.Count == 0)
                {
                    allMessages.Add(new ChatMessage
                    {
                        Role = "system",
                        Content = @"Bạn là chatbot hỗ trợ tư vấn khóa học cho nền tảng eLearning nội bộ. 
Nhiệm vụ của bạn:
1. Luôn ưu tiên sử dụng dữ liệu mà hệ thống đã gửi kèm (ví dụ các thẻ html chứa khóa học, giá tiền, tag). Nếu kết quả đã được hiển thị ở tin nhắn trước, hãy tóm tắt hoặc gợi ý cách người dùng mở chi tiết thay vì bịa thêm dữ liệu mới.
2. Khi người dùng hỏi về nhu cầu học (ví dụ ""có khóa C# cơ bản không"", ""có khóa miễn phí không""), hãy đặt câu hỏi bổ sung để làm rõ mục tiêu, trình độ, hình thức học, thời lượng mong muốn, ngân sách.
3. Quy ước giá: giá trị `null` hoặc `0` nghĩa là khóa học miễn phí. Hãy thể hiện rõ điều này trong câu trả lời (ví dụ: ""khóa này miễn phí"" hoặc ""giá 0đ"").
4. Nếu người dùng nhắc đến ""miễn phí"", ""free"" hoặc muốn tiết kiệm chi phí, hãy nhấn mạnh các khóa miễn phí/giá thấp trước. Nếu chưa thấy kết quả miễn phí trong dữ liệu hiện có, hãy nói rõ rằng bạn chưa nhận được khóa miễn phí nào và hướng dẫn người dùng cung cấp thêm từ khóa cụ thể (ngôn ngữ, chủ đề, trình độ).
5. Luôn cảnh báo khi bạn không chắc chắn hoặc không có dữ liệu. Đừng bịa thông tin khóa học, giá, ưu đãi hoặc số lượng bài học.
6. Giữ giọng điệu thân thiện, súc tích, ưu tiên tiếng Việt. Khi cần liệt kê nhiều bước, dùng danh sách đánh số hoặc dấu gạch đầu dòng để người dùng dễ đọc."
                    });
                }
            else
            {
                bool hasSystemMessage = messages.Any(m => string.Equals(m.Role, "system", StringComparison.OrdinalIgnoreCase));
                if (!hasSystemMessage)
                {
                    allMessages.Add(new ChatMessage
                    {
                        Role = "system",
                            Content = @"Bạn là chatbot hỗ trợ tư vấn khóa học cho nền tảng eLearning nội bộ. 
Nhiệm vụ của bạn:
1. Luôn ưu tiên sử dụng dữ liệu mà hệ thống đã gửi kèm (ví dụ các thẻ html chứa khóa học, giá tiền, tag). Nếu kết quả đã được hiển thị ở tin nhắn trước, hãy tóm tắt hoặc gợi ý cách người dùng mở chi tiết thay vì bịa thêm dữ liệu mới.
2. Khi người dùng hỏi về nhu cầu học (ví dụ ""có khóa C# cơ bản không"", ""có khóa miễn phí không""), hãy đặt câu hỏi bổ sung để làm rõ mục tiêu, trình độ, hình thức học, thời lượng mong muốn, ngân sách.
3. Quy ước giá: giá trị `null` hoặc `0` nghĩa là khóa học miễn phí. Hãy thể hiện rõ điều này trong câu trả lời (ví dụ: ""khóa này miễn phí"" hoặc ""giá 0đ"").
4. Nếu người dùng nhắc đến ""miễn phí"", ""free"" hoặc muốn tiết kiệm chi phí, hãy nhấn mạnh các khóa miễn phí/giá thấp trước. Nếu chưa thấy kết quả miễn phí trong dữ liệu hiện có, hãy nói rõ rằng bạn chưa nhận được khóa miễn phí nào và hướng dẫn người dùng cung cấp thêm từ khóa cụ thể (ngôn ngữ, chủ đề, trình độ).
5. Luôn cảnh báo khi bạn không chắc chắn hoặc không có dữ liệu. Đừng bịa thông tin khóa học, giá, ưu đãi hoặc số lượng bài học.
6. Giữ giọng điệu thân thiện, súc tích, ưu tiên tiếng Việt. Khi cần liệt kê nhiều bước, dùng danh sách đánh số hoặc dấu gạch đầu dòng để người dùng dễ đọc."
                    });
                }
                allMessages.AddRange(messages);
            }

            var request = new GroqRequest
            {
                Model = "llama-3.1-8b-instant", // Model chính xác
                Messages = allMessages,
                Temperature = 0.7,
                MaxTokens = 1000
            };

            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://api.groq.com/openai/v1/chat/completions", content);
            var responseString = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var groqResponse = JsonConvert.DeserializeObject<GroqResponse>(responseString);
                return groqResponse?.Choices?.FirstOrDefault()?.Message?.Content ?? "Xin lỗi, tôi không thể trả lời lúc này.";
            }
            else
            {
                return $"Lỗi: {response.StatusCode} - {responseString}";
            }
        }
        catch (Exception ex)
        {
            return $"Có lỗi xảy ra: {ex.Message}";
        }
    }
}