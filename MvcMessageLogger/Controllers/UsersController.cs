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
            var activeUser = _context.Users.Where(u => u.LoggedIn == true).Include(u => u.Following).FirstOrDefault();
            ViewData["ActiveUser"] = activeUser;

            var user = _context.Users.Where(u => u.Id == id).Include(u => u.Followers).Include(u => u.Following).Include(u => u.Messages).Single();
            return View(user);
        }

        [HttpPost]
        [Route("/users/logout")]
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
        [Route("/users/{id:int}/editcheck")]
        public IActionResult EditPasswordCheck(int id, string? error)
        {
            var activeUser = _context.Users.Where(u => u.LoggedIn == true).FirstOrDefault();
            ViewData["ActiveUser"] = activeUser;
            var user = _context.Users.Where(u => u.Id == id).Include(u => u.Messages).Single();
            ViewData["Error"] = error;
            return View(user);
        }
        [HttpPost]
        [Route("/users/{id:int}/editcheck")]
        public IActionResult EditForPasswordCheck(int id, string password)
        {
            string redirectString = $"/users/{id}/editcheck?error=true";
            var user = _context.Users.Where(u => u.Id == id).Include(u => u.Messages).Single();
            if (user.PasswordCheck(password))
            {
                redirectString = $"/users/{id}/edit?pcode={user.Encrypt(password)}";
            }
            return Redirect(redirectString);
        }
        [Route("/users/{id:int}/edit")]
        public IActionResult Edit(int id, string? pcode)
        {
            var activeUser = _context.Users.Where(u => u.LoggedIn == true).FirstOrDefault();
            ViewData["ActiveUser"] = activeUser;
            var user = _context.Users.Where(u => u.Id == id).Include(u => u.Messages).Single();
            ViewData["PCode"] = pcode;
            return View(user);
        }
        [HttpPost]
        [Route("/users/{id:int}")]
        public IActionResult Update(int id, User user)
        {
            user.Id = id;
            user.Password = user.Encrypt(user.Password);
            _context.Users.Update(user);
            _context.SaveChanges();
            return Redirect($"/users/{id}");
        }

        [HttpPost]
        [Route("users/{id:int}/delete")]
        public IActionResult Delete(int id)
        {
            var user = _context.Users.Where(u => u.Id == id).Include(u => u.Messages).Single();
            _context.Users.Remove(user);
            _context.SaveChanges();
            return Redirect("/users");
        }
        [HttpPost]
        [Route("users/{id:int}/follow")]
        public IActionResult Follow(int id)
        {
            var activeUser = _context.Users.Where(u => u.LoggedIn == true).FirstOrDefault();
            var user = _context.Users.Where(u => u.Id == id).Include(u => u.Messages).Single();
            activeUser.Following.Add(user);
            user.Followers.Add(activeUser);
            _context.Users.Update(activeUser);
            _context.Users.Update(user);
            _context.SaveChanges();

            return Redirect($"/users/{id}");
        }
        [Route("users/{id:int}/following")]
        public IActionResult Following(int id)
        {
            var activeUser = _context.Users.Where(u => u.LoggedIn == true).FirstOrDefault();
            ViewData["ActiveUser"] = activeUser;
            var user = _context.Users.Where(u => u.Id == id).Include(u => u.Following).Single();
            return View(user);
        }
        [Route("users/{id:int}/followers")]
        public IActionResult Followers(int id)
        {
            var activeUser = _context.Users.Where(u => u.LoggedIn == true).FirstOrDefault();
            ViewData["ActiveUser"] = activeUser;
            var user = _context.Users.Where(u => u.Id == id).Include(u => u.Followers).Single();
            return View(user);
        }
        public IActionResult Feed()
        {
            var activeUser = _context.Users.Where(u => u.LoggedIn == true).Include(u=> u.Following).ThenInclude(u => u.Messages).FirstOrDefault();
            ViewData["ActiveUser"] = activeUser;
            var allMessages = new List<Message>();
            foreach(var user in activeUser.Following)
            {
                allMessages.AddRange(user.Messages);
            }
            return View(allMessages.OrderBy(m => m.CreatedAt).ToList());
        }
    }
}
