namespace Dialogix.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public string User { get; set; } = "User";

        public string Text { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
