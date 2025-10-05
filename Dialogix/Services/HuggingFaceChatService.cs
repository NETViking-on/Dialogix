using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Dialogix.Services
{
    public class HuggingFaceChatService
    {
        private readonly HttpClient _httpClient;

        public HuggingFaceChatService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;

            var apiKey = configuration["HuggingFace:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentException("HuggingFace API key not found!");

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);
        }

        public async Task<string> GetResponseAsync(string message)
        {
            var payload = new { inputs = message };

            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            
            var response = await _httpClient.PostAsync(
                "https://api-inference.huggingface.co/models/gpt2",
                content);

            var responseBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"HTTP {response.StatusCode}: {responseBody}");

            response.EnsureSuccessStatusCode();

            using var doc = JsonDocument.Parse(responseBody);
            if (doc.RootElement.ValueKind == JsonValueKind.Array && doc.RootElement.GetArrayLength() > 0)
            {
                return doc.RootElement[0].GetProperty("generated_text").GetString() ?? "";
            }

            return "No generated_text found in response";
        }
    }
}
