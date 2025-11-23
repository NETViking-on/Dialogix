using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Dialogix.Models;
using Dialogix.Services;
using Microsoft.AspNetCore.Authorization; 
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dialogix.Pages
{
    [Authorize] 
    [IgnoreAntiforgeryToken] 
    public class ChatModel : PageModel
    {
        private readonly Data.IChatRepository _chatRepository;
        private readonly IBotService _botService;

        public List<ChatMessage> Messages { get; set; } = new();

        public ChatModel(Data.IChatRepository chatRepository, IBotService botService)
        {
            _chatRepository = chatRepository;
            _botService = botService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
           
            if (!User.Identity?.IsAuthenticated ?? false)
            {
                return RedirectToPage("/Login", new { returnUrl = "/Chat" });
            }

            Messages = (await _chatRepository.GetAllMessagesAsync()).ToList();
            return Page();
        }

        public async Task<IActionResult> OnPostSendMessageAsync([FromBody] ChatInputDto dto)
        {
            if (!User.Identity?.IsAuthenticated ?? false)
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(dto.Text))
                return new JsonResult(new { response = "Введите сообщение" });

            var userMessage = new ChatMessage
            {
                User = User.Identity?.Name ?? "You",
                Text = dto.Text,
                CreatedAt = DateTime.UtcNow
            };
            await _chatRepository.AddMessageAsync(userMessage);

            string botReply;
            try
            {
                botReply = await _botService.GetBotResponseAsync(dto.Text) ?? "Бот не ответил";
            }
            catch
            {
                botReply = "Ошибка в боте";
            }

            var botMessage = new ChatMessage
            {
                User = "Bot",
                Text = botReply,
                CreatedAt = DateTime.UtcNow
            };
            await _chatRepository.AddMessageAsync(botMessage);

            return new JsonResult(new { response = botReply });
        }
    }

    public class ChatInputDto
    {
        public string Text { get; set; } = string.Empty;
    }
}