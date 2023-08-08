using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcMessageLogger.DataAccess;
using MvcMessageLogger.Models;
using System.Diagnostics;

namespace MvcMessageLogger.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly MvcMessageLoggerContext _context;
        

        public HomeController(ILogger<HomeController> logger, MvcMessageLoggerContext context)
        {
            _logger = logger;
            _context = context;
        }



        public IActionResult Index()
        {
            var activeUser = _context.Users.Where(u => u.LoggedIn == true).FirstOrDefault();
            ViewData["ActiveUser"] = activeUser;

            return View();
        }

        public IActionResult Privacy()
        {
            var activeUser = _context.Users.Where(u => u.LoggedIn == true).FirstOrDefault();
            ViewData["ActiveUser"] = activeUser;

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}