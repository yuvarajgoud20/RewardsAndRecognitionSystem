using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RewardsAndRecognitionSystem.Models;
using Microsoft.Extensions.Localization;

namespace RewardsAndRecognitionSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IStringLocalizer _localizer;

        public HomeController(ILogger<HomeController> logger, IStringLocalizer localizer)
        {
            _logger = logger;
            _localizer = localizer;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { ErrorMessage= Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
