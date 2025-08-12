using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace RewardsAndRecognitionSystem.Controllers
{
    public class CultureController : Controller
    {
        [HttpPost]
        public IActionResult SetCulture(string culture, string returnUrl)
        {
            if (string.IsNullOrWhiteSpace(culture))
            {
                culture = "en-US"; // default to English if empty
            }

            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddYears(1),
                    IsEssential = true // make sure it's set for GDPR compliance
                }
            );

            // Prevent open redirect attacks
            if (!Url.IsLocalUrl(returnUrl))
            {
                returnUrl = Url.Action("Index", "Home")!;
            }

            return LocalRedirect(returnUrl);
        }
    }
}
