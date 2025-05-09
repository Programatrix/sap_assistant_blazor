namespace SAPAssistant.Models
{
    public class QueryResponse
    {
        public string Sql { get; set; }
        public string Resumen { get; set; }
        public List<Dictionary<string, object>> Resultados { get; set; }
    }

}
