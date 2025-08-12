using System.Collections.Concurrent;
using System.Data;
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

        private static ConcurrentDictionary<string, string> ConnectedUsers = new ConcurrentDictionary<string, string>();

        public async Task RegisterUser(string username)
        {
            if (!string.IsNullOrEmpty(username))
                ConnectedUsers[username] = Context.ConnectionId;

            await Clients.All.SendAsync("UserListUpdated", GetAllUsers());
        }

        public override async Task OnConnectedAsync()
        {
            string? username = Context.User?.Identity?.Name;

            if (string.IsNullOrEmpty(username))
            {
                username = Context.GetHttpContext()?.Request.Query["username"];
            }

            if (string.IsNullOrEmpty(username))
            {
                await Clients.Caller.SendAsync("Error", "Username is required to connect.");
                Context.Abort();
                return;
            }

            ConnectedUsers[username] = Context.ConnectionId;

            var unreadMessages = _context.MessageQueues
                .Where(m => m.Recipient == username && !m.IsRead)
                .OrderBy(m => m.CreatedAt)
                .ToList();

            foreach (var msg in unreadMessages)
            {
                await Clients.Client(Context.ConnectionId)
                    .SendAsync("ReceiveMessage", msg.UserName, msg.Message, msg.CreatedAt);

                var chatMessage = new ChatMessage
                {
                    UserName = msg.UserName,
                    Recipient = msg.Recipient,
                    Message = msg.Message,
                    SentAt = msg.CreatedAt,
                    IsRead = true
                };
                _context.ChatMessages.Add(chatMessage);

                msg.IsRead = true;
            }

            if (unreadMessages.Count > 0)
            {
                await _context.SaveChangesAsync();
            }

            await Clients.All.SendAsync("UserListUpdated", GetAllUsers());

            await base.OnConnectedAsync();
        }


        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var user = ConnectedUsers.FirstOrDefault(x => x.Value == Context.ConnectionId).Key;
            if (!string.IsNullOrEmpty(user))
            {
                ConnectedUsers.TryRemove(user, out _);
                await Clients.All.SendAsync("UserListUpdated", GetAllUsers());
            }
            await base.OnDisconnectedAsync(exception);
        }

        public List<string> GetAllUsers()
        {
            return _context.Users!
                .OrderBy(u => u.Username)
                .Select(u => u.Username!)
                .ToList();
        }

        public async Task SendMessage(string sender, string role, string recipient, string message)
        {
            if (role == "Admin")
            {
                var chatMessage = new ChatMessage
                {
                    UserName = sender,
                    Message = message,
                    SentAt = DateTime.UtcNow
                };

                _context.ChatMessages.Add(chatMessage);
                await _context.SaveChangesAsync();

                await Clients.All.SendAsync("ReceiveMessage", sender, message, chatMessage.SentAt);
            }
            else
            {
                if (ConnectedUsers.TryGetValue(recipient, out var connectionId) && !string.IsNullOrEmpty(connectionId))
                {
                    var chatMessage = new ChatMessage
                    {
                        UserName = sender,
                        Message = message,
                        Recipient = recipient,
                        SentAt = DateTime.UtcNow
                    };

                    _context.ChatMessages.Add(chatMessage);
                    await _context.SaveChangesAsync();

                    await Clients.Client(connectionId).SendAsync("ReceiveMessage", sender, message, chatMessage.SentAt);
                }
                else
                {
                    var queuedMessage = new MessageQueue
                    {
                        UserName = sender,
                        Recipient = recipient,
                        Message = message,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.MessageQueues.Add(queuedMessage);
                    await _context.SaveChangesAsync();
                }
            }
        }

    }
}

