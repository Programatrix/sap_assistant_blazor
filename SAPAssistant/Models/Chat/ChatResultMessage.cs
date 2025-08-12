namespace SAPAssistant.Models.Chat
{
    public class ChatResultMessage : MessageBase
    {
        public string Resumen { get; set; } = string.Empty;
        public string Sql { get; set; } = string.Empty;
        public List<Dictionary<string, object>> Data { get; set; } = new();
        public string ViewType { get; set; } = "grid";
        public override string Type => "Result";
    }
}
