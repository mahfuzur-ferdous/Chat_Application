using Chat_Application.Data;
using Chat_Application.Entity;
using Microsoft.AspNetCore.SignalR;

namespace Chat_Application.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _context;
        public ChatHub(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task SendMessage(string user, string message)
        {
            var chatMessage = new ChatMessage
            {
                UserName = user,
                Message = message,
                SentAt = DateTime.UtcNow
            };

            _context.ChatMessages.Add(chatMessage);
            await _context.SaveChangesAsync();
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}
