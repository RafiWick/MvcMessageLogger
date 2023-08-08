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

        [Route("/users/{userId:int}/messages/{messageId:int}/edit")]
        public IActionResult Edit(int userId, int messageId)
        {
            var message = _context.Messages.Find(messageId);
            var user = _context.Users.Where(u => u.Id == id).Include(u => u.Messages).Single();
            message.Author = user;
            return View(message);
        }

        [HttpPost]
        [Route("/users/{userId:int}/messages/{messageId:int}")]
        public IActionResult Update(int userId, int messageId, string content)
        {
            var user = _context.Users.Where(u => u.Id == id).Include(u => u.Messages).Single();
            var message = _context.Messages.Find(messageId);
            message.Content = content;
            message.Edited = true;
            _context.Messages.Update(message)
            return Redirect($"/users/{userId}");
        }

        [HttpPost]
        [Route("/users/{userId:int}/messages/{messageId:int}/delete")]
        public IActionResult Delete(int userId, int messageId)
        {
            var message = _context.Messages.Find(messageId);
            _context.Messages.Remove(message);
            _context.SaveChanges();
            return Redirect($"/users/{userId}");
        }
    }
}
