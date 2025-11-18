using Dialogix.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Dialogix.Pages;

public class LoginModel : PageModel
{
    private readonly ChatDbContext _context;

    public LoginModel(ChatDbContext context)
    {
        _context = context;
    }

    [BindProperty] public string Email { get; set; } = string.Empty;
    [BindProperty] public string Password { get; set; } = string.Empty;
    [BindProperty] public string? ReturnUrl { get; set; }

    // === USER INFO (KZ, 10 ÌÓˇ·ˇ 2025) ===
    public string CurrentTime => TimeZoneInfo.ConvertTimeFromUtc(
        DateTime.UtcNow,
        TimeZoneInfo.FindSystemTimeZoneById("Central Asia Standard Time")
    ).ToString("MMMM d, yyyy h:mm tt") + " +05";

    public string Country => "KZ";

    public void OnGet(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;

        if (!ModelState.IsValid)
            return Page();

        var passwordHash = HashPassword(Password);
        var user = _context.Users.FirstOrDefault(u =>
            u.Email.ToLower() == Email.ToLower() && u.PasswordHash == passwordHash);

        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return Page();
        }

        // ¿¬“Œ–»«¿÷»ﬂ ◊≈–≈« COOKIE (Õ≈ Session!)
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            });

        // –≈ƒ»–≈ “
        return !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? Redirect(returnUrl)
            : RedirectToPage("/Chat");
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}