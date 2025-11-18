using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using Dialogix.Data;
using Dialogix.Models;

namespace Dialogix.Pages
{
    [Authorize]
    public class ProfileModel : PageModel
    {
        private readonly IChatRepository _chatRepository;
        private readonly ILogger<ProfileModel> _logger;

        public ProfileModel(IChatRepository chatRepository, ILogger<ProfileModel> logger)
        {
            _chatRepository = chatRepository;
            _logger = logger;
        }

        // Основная информация пользователя
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime MemberSince { get; set; }

        // Статистика чатов
        public int TotalMessages { get; set; }
        public int TodayMessages { get; set; }
        public int ThisWeekMessages { get; set; }
        public int ThisMonthMessages { get; set; }

        // Активность
        public List<ChatMessage> RecentMessages { get; set; } = new();
        public Dictionary<string, int> MessagesPerDay { get; set; } = new();
        public Dictionary<string, int> HourlyActivity { get; set; } = new();

        // Аналитика
        public int AverageMessageLength { get; set; }
        public string MostActiveDay { get; set; } = "No data";
        public string MostActiveHour { get; set; } = "No data";
        public int LongestConversation { get; set; }

        // Достижения
        public List<Achievement> Achievements { get; set; } = new();
        public int AchievementProgress { get; set; }

        [BindProperty]
        public ProfileUpdateModel ProfileUpdate { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Login");
            }

            await LoadUserData();
            await CalculateStatistics();
            await LoadRecentActivity();
            await CalculateAnalytics();
            await LoadAchievements();

            return Page();
        }

        public async Task<IActionResult> OnPostUpdateProfileAsync()
{
    if (!ModelState.IsValid)
    {
        await LoadUserData();
        return Page();
    }

    try
    {
        var user = await _chatRepository.GetUserByUsernameAsync(User.Identity.Name);
        if (user != null)
        {
            // Обновляем только если переданы значения
            if (!string.IsNullOrEmpty(ProfileUpdate.DisplayName?.Trim()))
            {
                user.DisplayName = ProfileUpdate.DisplayName.Trim();
            }
            
            if (!string.IsNullOrEmpty(ProfileUpdate.Bio?.Trim()))
            {
                user.Bio = ProfileUpdate.Bio.Trim();
            }

            await _chatRepository.UpdateUserAsync(user);
            TempData["SuccessMessage"] = "Profile updated successfully!";
            
            // Обновляем данные на странице
            await LoadUserData();
        }
        else
        {
            TempData["ErrorMessage"] = "User not found.";
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error updating profile");
        TempData["ErrorMessage"] = "Error updating profile.";
    }

    return Page();
}

        public async Task<IActionResult> OnPostExportDataAsync()
        {
            var userData = new
            {
                Username,
                Email,
                MemberSince,
                Statistics = new
                {
                    TotalMessages,
                    TodayMessages,
                    AverageMessageLength,
                    MostActiveDay,
                    MostActiveHour
                },
                RecentMessages = RecentMessages.Take(10).Select(m => new
                {
                    m.Text,
                    m.CreatedAt,
                    m.User
                })
            };

            var json = System.Text.Json.JsonSerializer.Serialize(userData, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });

            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            return File(bytes, "application/json", $"{Username}_data_{DateTime.Now:yyyyMMdd_HHmmss}.json");
        }

        private async Task LoadUserData()
        {
            var user = await _chatRepository.GetUserByUsernameAsync(User.Identity.Name);
            if (user != null)
            {
                // Используем DisplayName если есть, иначе обычный Username
                Username = !string.IsNullOrEmpty(user.DisplayName) ? user.DisplayName : user.Username;
                Email = user.Email;
                MemberSince = user.CreatedAt;

                // Предзаполняем форму текущими значениями
                if (ProfileUpdate == null)
                {
                    ProfileUpdate = new ProfileUpdateModel();
                }

                ProfileUpdate.DisplayName = user.DisplayName ?? user.Username;
                ProfileUpdate.Bio = user.Bio ?? "";
            }
            else
            {
                Username = User.Identity.Name ?? "Unknown";
                Email = $"{Username}@example.com";
                MemberSince = DateTime.UtcNow.AddMonths(-1);

                if (ProfileUpdate == null)
                {
                    ProfileUpdate = new ProfileUpdateModel();
                }
                ProfileUpdate.DisplayName = Username;
            }
        }

        private async Task CalculateStatistics()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username)) return;

            // Используем UTC даты
            var today = DateTime.UtcNow.Date;
            var weekStart = today.AddDays(-(int)today.DayOfWeek);
            var monthStart = new DateTime(today.Year, today.Month, 1);

            // Убедимся, что все даты в UTC
            var todayUtc = DateTime.SpecifyKind(today, DateTimeKind.Utc);
            var weekStartUtc = DateTime.SpecifyKind(weekStart, DateTimeKind.Utc);
            var monthStartUtc = DateTime.SpecifyKind(monthStart, DateTimeKind.Utc);

            TotalMessages = await _chatRepository.GetUserMessageCountAsync(username);
            TodayMessages = await _chatRepository.GetUserMessageCountByPeriodAsync(username, todayUtc);
            ThisWeekMessages = await _chatRepository.GetUserMessageCountByPeriodAsync(username, weekStartUtc);
            ThisMonthMessages = await _chatRepository.GetUserMessageCountByPeriodAsync(username, monthStartUtc);

            MessagesPerDay = await _chatRepository.GetMessagesPerDayAsync(username);
            HourlyActivity = await _chatRepository.GetHourlyActivityAsync(username);
        }

        private async Task LoadRecentActivity()
        {
            var username = User.Identity.Name;
            if (!string.IsNullOrEmpty(username))
            {
                RecentMessages = await _chatRepository.GetUserMessagesAsync(username, 10);
            }
        }

        private async Task CalculateAnalytics()
        {
            var username = User.Identity.Name;
            if (string.IsNullOrEmpty(username)) return;

            AverageMessageLength = (int)await _chatRepository.GetAverageMessageLengthAsync(username);
            MostActiveDay = await _chatRepository.GetMostActiveDayAsync(username);
            MostActiveHour = await _chatRepository.GetMostActiveHourAsync(username);

            // Расчет самой длинной беседы
            var messages = await _chatRepository.GetUserMessagesAsync(username);
            LongestConversation = CalculateLongestConversation(messages);
        }

        private int CalculateLongestConversation(List<ChatMessage> messages)
        {
            if (!messages.Any()) return 0;

            var orderedMessages = messages.OrderBy(m => m.CreatedAt).ToList();
            int maxLength = 1;
            int currentLength = 1;

            for (int i = 1; i < orderedMessages.Count; i++)
            {
                var timeDiff = orderedMessages[i].CreatedAt - orderedMessages[i - 1].CreatedAt;
                if (timeDiff.TotalMinutes <= 30)
                {
                    currentLength++;
                    maxLength = Math.Max(maxLength, currentLength);
                }
                else
                {
                    currentLength = 1;
                }
            }

            return maxLength;
        }

        private async Task LoadAchievements()
        {
            var username = User.Identity.Name;
            if (string.IsNullOrEmpty(username)) return;

            var achievements = new List<Achievement>
            {
                new Achievement {
                    Name = "First Message",
                    Description = "Send your first message",
                    Icon = "🎯",
                    IsUnlocked = TotalMessages >= 1,
                    Progress = TotalMessages >= 1 ? 100 : 0
                },
                new Achievement {
                    Name = "Chatterbox",
                    Description = "Send 100 messages",
                    Icon = "💬",
                    IsUnlocked = TotalMessages >= 100,
                    Progress = Math.Min(TotalMessages, 100)
                },
                new Achievement {
                    Name = "Early Bird",
                    Description = "Send messages before 8 AM",
                    Icon = "🌅",
                    IsUnlocked = HourlyActivity.ContainsKey("06") && HourlyActivity["06"] > 0,
                    Progress = HourlyActivity.ContainsKey("06") && HourlyActivity["06"] > 0 ? 100 : 0
                },
                new Achievement {
                    Name = "Night Owl",
                    Description = "Send messages after 10 PM",
                    Icon = "🌙",
                    IsUnlocked = HourlyActivity.ContainsKey("22") && HourlyActivity["22"] > 0,
                    Progress = HourlyActivity.ContainsKey("22") && HourlyActivity["22"] > 0 ? 100 : 0
                },
                new Achievement {
                    Name = "Consistent",
                    Description = "Send messages for 7 consecutive days",
                    Icon = "📅",
                    IsUnlocked = MessagesPerDay.Values.Count(v => v > 0) >= 7,
                    Progress = (MessagesPerDay.Values.Count(v => v > 0) * 100) / 7
                }
            };

            Achievements = achievements;
            AchievementProgress = achievements.Count(a => a.IsUnlocked) * 100 / Math.Max(achievements.Count, 1);
        }
    }

    public class ProfileUpdateModel
    {
        [StringLength(50, ErrorMessage = "Display name must be less than 50 characters")]
        public string DisplayName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Bio must be less than 500 characters")]
        public string Bio { get; set; } = string.Empty;
    }

    public class Achievement
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public bool IsUnlocked { get; set; }
        public int Progress { get; set; }
    }
}