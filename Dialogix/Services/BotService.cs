using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Dialogix.Services
{
    public class BotService : IBotService
    {
        private readonly HttpClient _httpClient;

        public BotService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;

            var apiKey = configuration["HuggingFace:ApiKey"];
        

        

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);
        }

        public async Task<string?> GetBotResponseAsync(string userMessage)
        {
            if (string.IsNullOrWhiteSpace(userMessage))
                return "Please say something";

            try
            {
                var payload = new
                {
                    model = "deepseek-ai/DeepSeek-V3-0324",
                    messages = new[]
                    {
                        new { role = "user", content = userMessage }
                    }
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync(
                    "https://router.huggingface.co/v1/chat/completions",
                    content);

                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"HTTP {response.StatusCode}: {responseBody}");

                if (!response.IsSuccessStatusCode)
                    return $"(Error {response.StatusCode})";

                using var doc = JsonDocument.Parse(responseBody);
                if (doc.RootElement.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
                {
                    var message = choices[0].GetProperty("message");
                    if (message.TryGetProperty("content", out var contentElement))
                        return contentElement.GetString();
                }

                return "No generated_text found in response";
            }
            catch (System.Exception ex)
            {
                return $"Failed to get response from AI server: {ex.Message}";
            }
        }
    }
}
