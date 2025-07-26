namespace SAPAssistant.Models.Dashboard
{
    public class DashboardWidget
    {
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public Guid Id { get; set; } = Guid.NewGuid();
        // Aquí puedes añadir más propiedades (ej: posición, SQL, configuración, etc.)
    }

}
