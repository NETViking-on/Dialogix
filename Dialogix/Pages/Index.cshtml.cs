using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Dialogix.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;

    public string WelcomeMessage { get; private set; } = string.Empty;
    public bool IsLoggedIn => User.Identity?.IsAuthenticated ?? false;
    public string Username => User.Identity?.Name ?? "Guest";

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {
        WelcomeMessage = "Welcome to Dialogix – your chatbot playground!";
        _logger.LogInformation("Index page visited at {Time} by {User}", DateTime.UtcNow, Username);
    }
}