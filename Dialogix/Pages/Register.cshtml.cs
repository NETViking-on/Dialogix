using Dialogix.Data;
using Dialogix.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Cryptography;
using System.Text;

namespace Dialogix.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly ChatDbContext _context;

        public RegisterModel(ChatDbContext context)
        {
            _context = context;
        }

        [BindProperty] public string Username { get; set; } = string.Empty;
        [BindProperty] public string Email { get; set; } = string.Empty;
        [BindProperty] public string Password { get; set; } = string.Empty;

        public void OnGet() { }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
                return Page();

            // Проверяем дубликат email
            if (_context.Users.Any(u => u.Email == Email))
            {
                ModelState.AddModelError(string.Empty, "User with this email already exists.");
                return Page();
            }

            var passwordHash = HashPassword(Password);

            var user = new User
            {
                Username = Username,
                Email = Email,
                PasswordHash = passwordHash
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            // После регистрации — авторизуем и переходим в чат
            HttpContext.Session.SetString("UserEmail", Email);
            HttpContext.Session.SetString("Username", Username);

            return RedirectToPage("/Chat");
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}
