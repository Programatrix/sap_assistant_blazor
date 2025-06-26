namespace SAPAssistant.Models.Chat
{
    public class ResultMessage : MessageBase
    {
        public string Resumen { get; set; } = string.Empty;
        public string Sql { get; set; } = string.Empty;
        public List<Dictionary<string, object>> Data { get; set; } = new();
        public override string Type => "Result";
    }
}
