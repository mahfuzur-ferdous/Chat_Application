namespace Chat_Application.Entity
{
    public class MessageQueue
    {
        public int Id { get; set; }
        public string? UserName { get; set; }
        public string? Recipient { get; set; }
        public string? Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; } = false;
    }
}
