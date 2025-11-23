using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Localization;
using System.Globalization;

namespace Dialogix.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IStringLocalizer<IndexModel> _localizer;

        public IndexModel(IStringLocalizer<IndexModel> localizer)
        {
            _localizer = localizer;
        }

        public bool IsLoggedIn => User.Identity?.IsAuthenticated ?? false;
        public string Username => User.Identity?.Name ?? "User";
        public string CurrentCulture => CultureInfo.CurrentCulture.Name;

        public void OnGet()
        {
        }

        public IActionResult OnPostSetLanguage(string culture, string returnUrl)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddYears(1),
                    IsEssential = true
                }
            );

            return LocalRedirect(returnUrl);
        }
    }
}