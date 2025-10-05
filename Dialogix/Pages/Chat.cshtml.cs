using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Dialogix.Models;
using Dialogix.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dialogix.Pages
{
    [IgnoreAntiforgeryToken] 
    public class ChatModel : PageModel
    {
        private readonly IChatRepository _chatRepository;
        private readonly IBotService _botService;

        public List<ChatMessage> Messages { get; set; } = new();

        public ChatModel(IChatRepository chatRepository, IBotService botService)
        {
            _chatRepository = chatRepository;
            _botService = botService;
        }

        public async Task OnGetAsync()
        {
            Messages = (await _chatRepository.GetAllMessagesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostSendMessageAsync([FromBody] ChatInputDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Text))
                return new JsonResult(new { response = "Введите сообщение" });

           
            var userMessage = new ChatMessage
            {
                User = "You",
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
