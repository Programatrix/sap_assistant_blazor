namespace SAPAssistant.Models.Chat
{
    public abstract class MessageBase
    {
        public bool IsUser { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public abstract string Type { get; }
    }
}
