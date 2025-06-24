namespace SAPAssistant.Models.Chat
{
    public abstract class MessageBase
    {
        public string Role { get; set; } = "assistant";  // "user" o "assistant"
        public bool IsUser => Role.Equals("user", StringComparison.OrdinalIgnoreCase);        
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public abstract string Type { get; }
    }
}
