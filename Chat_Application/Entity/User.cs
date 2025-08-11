namespace Chat_Application.Entity
{
    public class User
    {
        public int Id { get; set; }
        public string? Role { get; set; }
        public string? Username { get; set; }
        public string? PasswordHash { get; set; }
    }

}
