using Dialogix.Models;

namespace Dialogix.Data
{
    public interface IChatRepository
    {
        // Users
        Task<User?> GetUserByUsernameAsync(string username);
        Task<User?> GetUserByEmailAsync(string email);
        Task<User> CreateUserAsync(User user);
        Task UpdateUserAsync(User user);
        Task<bool> UserExistsAsync(string username, string email);

        // Chat Messages
        Task<List<ChatMessage>> GetRecentMessagesAsync(int count = 50);
        Task<List<ChatMessage>> GetAllMessagesAsync(); // ← ДОБАВЬ ЭТОТ МЕТОД
        Task<ChatMessage> AddMessageAsync(ChatMessage message);
        Task<List<ChatMessage>> GetUserMessagesAsync(string username, int? count = null);
        Task<int> GetUserMessageCountAsync(string username);
        Task<int> GetUserMessageCountByPeriodAsync(string username, DateTime start, DateTime? end = null);

        // Analytics
        Task<Dictionary<string, int>> GetMessagesPerDayAsync(string username, int days = 7);
        Task<Dictionary<string, int>> GetHourlyActivityAsync(string username);
        Task<double> GetAverageMessageLengthAsync(string username);
        Task<string> GetMostActiveDayAsync(string username);
        Task<string> GetMostActiveHourAsync(string username);

        // Additional methods
        Task<User?> GetUserByIdAsync(int id);
        Task UpdateUserProfileAsync(int userId, string? displayName, string? bio);
    }
}