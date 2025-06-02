namespace SAPAssistant.Models.Chat
{
    public class SystemMessage : MessageBase
    {
        public string Mensaje { get; set; }
        public override string Type => "system";        
    }
}
