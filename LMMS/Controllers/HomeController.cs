using System.Diagnostics;
using LMMS.Models;
using Microsoft.AspNetCore.Mvc;

namespace LMMS.Controllers
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
            return View();
        }
        public IActionResult Profile()   // profile Page
        {
            return View();
        }
        public IActionResult Report()   // Report Page
        {
            return View();
        }
        public IActionResult Manage()   // manage Page
        {
            return View();
        }
        public IActionResult Books()   // Books Page
        {
            return View();
        }
        public IActionResult RentedBooks()   // Books Page
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
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
