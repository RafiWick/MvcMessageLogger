﻿using Microsoft.AspNetCore.Mvc;
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
    }
}