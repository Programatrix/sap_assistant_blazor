namespace SAPAssistant.Models
{
    public class ConnectionDTO
    {
        public string ConnectionId { get; set; } = "";
        public string Host { get; set; } = "";
        public int Port { get; set; } = 0;
        public string User { get; set; } = "";
        public string Schema { get; set; } = "";
        public string Database { get; set; } = "";    
        public string Password { get; set; } = "";    
        public Boolean IsActive { get; set; } = false;
    }
}
