using Chat_Application.Entity;
using Microsoft.EntityFrameworkCore;

namespace Chat_Application.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<MessageQueue> MessageQueues { get; set; }
    }

}
