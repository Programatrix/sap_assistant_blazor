namespace SAPAssistant.Models.Chat
{
    public class ErrorMessage : MessageBase
    {
        public string Error { get; set; } = string.Empty;
        public override string Type => "Error";
    }
}
