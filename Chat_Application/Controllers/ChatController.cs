using Chat_Application.Data;
using Chat_Application.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Chat_Application.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ChatController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var username = User.Identity?.Name;
            var messages = _context.ChatMessages
                .Where(m => m.Recipient == username || string.IsNullOrEmpty(m.Recipient)) 
                .OrderBy(m => m.SentAt)
                .ToList();

            var role = User.IsInRole("Admin") ? "Admin" : "User";
            ViewBag.UserRole = role;
            ViewBag.Username = username;

            return View(messages);
        }
    }
}
