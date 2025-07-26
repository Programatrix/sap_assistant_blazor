namespace SAPAssistant.Models
{
    public class DashboardWidgetModel
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Type { get; set; } = "chart"; // chart, table, kpi, etc.
        public string Title { get; set; } = "";
        public string Sql { get; set; } = "";
        public string ChartType { get; set; } = "bar";
        public string Size { get; set; } = "medium";
        public Dictionary<string, string> Mappings { get; set; } = new();
      
        public double X { get; set; } = 0;
        public double Y { get; set; } = 0;

        public string Xpx => $"{X}px";
        public string Ypx => $"{Y}px";
    }

}
