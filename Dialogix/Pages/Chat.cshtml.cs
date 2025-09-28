using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Dialogix.Pages
{
    public class ChatModel : PageModel
    {
        [BindProperty]
        public string Message { get; set; } = string.Empty;

        public List<string> ChatHistory { get; private set; } = new();

        
        private static readonly List<string> _messages = new();

        public void OnGet()
        {
            ChatHistory = new List<string>(_messages);
        }

        public IActionResult OnPost()
        {
            if (!string.IsNullOrWhiteSpace(Message))
            {
                _messages.Add(Message);
            }

            return RedirectToPage(); 
        }
    }
}
