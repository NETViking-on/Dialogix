using Microsoft.EntityFrameworkCore;
using Dialogix.Models;

namespace Dialogix.Data
{
    public class ChatRepository : IChatRepository
    {
        private readonly ChatDbContext _context;

        public ChatRepository(ChatDbContext context)
        {
            _context = context;
        }

        // User operations
        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User> CreateUserAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> UserExistsAsync(string username, string email)
        {
            return await _context.Users
                .AnyAsync(u => u.Username == username || u.Email == email);
        }

        // Message operations
        public async Task<List<ChatMessage>> GetRecentMessagesAsync(int count = 50)
        {
            return await _context.ChatMessages
                .OrderByDescending(m => m.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<ChatMessage>> GetAllMessagesAsync()
        {
            return await _context.ChatMessages
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<ChatMessage> AddMessageAsync(ChatMessage message)
        {
            _context.ChatMessages.Add(message);
            await _context.SaveChangesAsync();
            return message;
        }

        public async Task<List<ChatMessage>> GetUserMessagesAsync(string username, int? count = null)
        {
            var query = _context.ChatMessages
                .Where(m => m.User == username)
                .OrderByDescending(m => m.CreatedAt);

            if (count.HasValue)
            {
                query = (IOrderedQueryable<ChatMessage>)query.Take(count.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<int> GetUserMessageCountAsync(string username)
        {
            return await _context.ChatMessages
                .Where(m => m.User == username)
                .CountAsync();
        }

        public async Task<int> GetUserMessageCountByPeriodAsync(string username, DateTime start, DateTime? end = null)
        {
            // Убедимся, что даты в UTC
            var startUtc = start.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(start, DateTimeKind.Utc)
                : start.ToUniversalTime();

            var endUtc = end.HasValue
                ? (end.Value.Kind == DateTimeKind.Unspecified
                    ? DateTime.SpecifyKind(end.Value, DateTimeKind.Utc)
                    : end.Value.ToUniversalTime())
                : (DateTime?)null;

            var query = _context.ChatMessages
                .Where(m => m.User == username && m.CreatedAt >= startUtc);

            if (endUtc.HasValue)
            {
                query = query.Where(m => m.CreatedAt <= endUtc.Value);
            }

            return await query.CountAsync();
        }

        // Analytics operations
        public async Task<Dictionary<string, int>> GetMessagesPerDayAsync(string username, int days = 7)
        {
            var startDate = DateTime.UtcNow.Date.AddDays(-days + 1);
            var startDateUtc = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);

            var messages = await _context.ChatMessages
                .Where(m => m.User == username && m.CreatedAt >= startDateUtc)
                .GroupBy(m => m.CreatedAt.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();

            var result = new Dictionary<string, int>();
            for (int i = 0; i < days; i++)
            {
                var date = startDate.AddDays(i);
                var dayName = date.ToString("ddd");
                var count = messages.FirstOrDefault(m => m.Date == date)?.Count ?? 0;
                result[dayName] = count;
            }

            return result;
        }

        public async Task<Dictionary<string, int>> GetHourlyActivityAsync(string username)
        {
            var messages = await _context.ChatMessages
                .Where(m => m.User == username)
                .GroupBy(m => m.CreatedAt.Hour)
                .Select(g => new { Hour = g.Key, Count = g.Count() })
                .ToListAsync();

            var result = new Dictionary<string, int>();
            for (int hour = 0; hour < 24; hour++)
            {
                var hourStr = hour.ToString("00");
                var count = messages.FirstOrDefault(m => m.Hour == hour)?.Count ?? 0;
                result[hourStr] = count;
            }

            return result;
        }

        public async Task<double> GetAverageMessageLengthAsync(string username)
        {
            var messages = await _context.ChatMessages
                .Where(m => m.User == username)
                .Select(m => m.Text.Length)
                .ToListAsync();

            return messages.Any() ? messages.Average() : 0;
        }

        public async Task<string> GetMostActiveDayAsync(string username)
        {
            var dayStats = await _context.ChatMessages
                .Where(m => m.User == username)
                .GroupBy(m => m.CreatedAt.DayOfWeek)
                .Select(g => new { Day = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .FirstOrDefaultAsync();

            return dayStats?.Day.ToString() ?? "No data";
        }

        public async Task<string> GetMostActiveHourAsync(string username)
        {
            var hourStats = await _context.ChatMessages
                .Where(m => m.User == username)
                .GroupBy(m => m.CreatedAt.Hour)
                .Select(g => new { Hour = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .FirstOrDefaultAsync();

            return hourStats != null ? $"{hourStats.Hour:00}:00" : "No data";
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task UpdateUserProfileAsync(int userId, string? displayName, string? bio)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                if (!string.IsNullOrEmpty(displayName))
                {
                    user.DisplayName = displayName;
                }
                if (!string.IsNullOrEmpty(bio))
                {
                    user.Bio = bio;
                }
                await _context.SaveChangesAsync();
            }
        }
    }
}