using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RewardsAndRecognitionSystem.Models;

namespace RewardsAndRecognitionSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
           //throw new Exception("Custom Exceptioon : I am testing");
            //_logger.LogInformation("Info log from Index");
            //_logger.LogWarning("Warning log from Index");
            //_logger.LogError("Error log from Index");

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
