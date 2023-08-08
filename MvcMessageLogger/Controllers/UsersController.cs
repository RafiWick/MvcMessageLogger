using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcMessageLogger.DataAccess;
using MvcMessageLogger.Models;

namespace MvcMessageLogger.Controllers
{
    public class UsersController : Controller
    {
        private readonly MvcMessageLoggerContext _context;
        public UsersController(MvcMessageLoggerContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var activeUser = _context.Users.Where(u => u.LoggedIn == true).FirstOrDefault();
            ViewData["ActiveUser"] = activeUser;

            var users = _context.Users.ToList();
            return View(users);
        }
        public IActionResult New()
        {
            return View();
        }
        [HttpPost]
        [Route("/users")]
        public IActionResult Create(User user)
        {
            user.Password = user.Encrypt(user.Password);
            _context.Users.Add(user);
            _context.SaveChanges();
            return Redirect("/users");
        }

        public IActionResult LogIn(bool? error)
        {
            if (error != null)
            {
                ViewData["Error"] = true;
            }
            return View();
        }

        [HttpPost]
        public IActionResult LogIn(Dictionary<string,string> login)
        {
            var redirectString = "/users/login?error=true";
            var user = _context.Users.Where(u => u.Email == login["Email"]).FirstOrDefault();
            if (user.PasswordCheck(login["Password"]))
            {
                user.LoggedIn = true;
                _context.Users.Update(user);
                _context.SaveChanges();
                redirectString = $"/users/{user.Id}";
            }
            return Redirect(redirectString);
        }

        [Route("/users/{id:int}")]
        public IActionResult Show(int id)
        {
            var activeUser = _context.Users.Where(u => u.LoggedIn == true).FirstOrDefault();
            ViewData["ActiveUser"] = activeUser;

            var user = _context.Users.Where(u => u.Id == id).Include(u => u.Messages).Single();
            return View(user);
        }

        [HttpPost]
        public IActionResult LogOut()
        {
            var user = _context.Users.Where(u => u.LoggedIn == true).FirstOrDefault();
            if (user != null)
            {
                user.LoggedIn = false;
                _context.Users.Update(user);
                _context.SaveChanges();
            }

            return Redirect("/users");
        }
    }
}
