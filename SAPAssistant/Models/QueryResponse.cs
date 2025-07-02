namespace SAPAssistant.Models
{
    public class QueryResponse
    {
        public string Tipo { get; set; } = "consulta"; // "consulta", "refinamiento", "aclaracion"
        public string Sql { get; set; }
        public string Resumen { get; set; }
        public string Mensaje { get; set; }
        public string ViewType { get; set; } = "grid";
        public List<Dictionary<string, object>> Resultados { get; set; }
    }
}
