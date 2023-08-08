using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcMessageLogger.DataAccess;
using MvcMessageLogger.Models;

namespace MvcMessageLogger.Controllers
{
    public class MessagesController : Controller
    {
        private readonly MvcMessageLoggerContext _context;
        public MessagesController(MvcMessageLoggerContext context)
        {
            _context = context;
        }

        [Route("/users/{id:int}/messages/new")]
        public IActionResult New(int id)
        {
            var activeUser = _context.Users.Where(u => u.LoggedIn == true).FirstOrDefault();
            ViewData["ActiveUser"] = activeUser;

            var user = _context.Users.Find(id);
            return View(user);
        }

        [HttpPost]
        [Route("/users/{id:int}/messages")]
        public IActionResult Create(int id, string content)
        {
            var user = _context.Users.Where(u => u.Id == id).Include(u => u.Messages).Single();
            var message = new Message { Content = content };
            message.CreatedAt = DateTime.Now.ToUniversalTime().Subtract(new TimeSpan(6, 0, 0));
            user.Messages.Add(message);
            _context.Users.Update(user);
            _context.SaveChanges();
            return Redirect($"/users/{id}");
        }
    }
}
