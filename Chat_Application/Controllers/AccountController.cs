using Chat_Application.Data;
using Chat_Application.Entity;
using Chat_Application.Infrastructure;
using Chat_Application.Migrations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Chat_Application.Controllers
{
    public class AccountController : Controller
    {
        private readonly TokenProvider _tokenProvider;
        private readonly ApplicationDbContext _context;
        public AccountController(TokenProvider tokenProvider, ApplicationDbContext context)
        {
            _tokenProvider = tokenProvider;
            _context = context;
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(string username, string password)
        {
            if (await _context.Users.AnyAsync(u => u.Username == username))
            {
                ModelState.AddModelError("", "Username already exists");
                return View();
            }

            bool isAdmin = await _context.Admins.AnyAsync(a => a.UserName == username);

            var user = new User
            {
                Username = username,
                Role = isAdmin ? "Admin" : "User",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("Login");
        }


        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                ModelState.AddModelError("", "Invalid username or password");
                return View();
            }

            var token = _tokenProvider.GenerateToken(user);

            Response.Cookies.Append("jwtToken", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(1)
            });

            return RedirectToAction("Index", "Chat");
        }

        [HttpPost]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("jwtToken");
            return RedirectToAction("Login");
        }


    }
}
