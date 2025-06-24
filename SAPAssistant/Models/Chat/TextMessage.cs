namespace SAPAssistant.Models.Chat
{
    public class TextMessage : MessageBase
    {
        public string Mensaje { get; set; } = string.Empty;
        public override string Type => "Text";
    }
}
