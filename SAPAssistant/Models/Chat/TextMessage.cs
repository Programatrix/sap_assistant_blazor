namespace SAPAssistant.Models.Chat
{
    public class TextMessage : MessageBase
    {
        public string Content { get; set; } = string.Empty;
        public override string Type => "Text";
    }
}
