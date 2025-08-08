using Chat_Application.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
            var messages = _context.ChatMessages
                .OrderBy(m => m.SentAt)
                .ToList();

            ViewBag.Username = User.Identity?.Name;

            return View(messages);
        }

    }
}
