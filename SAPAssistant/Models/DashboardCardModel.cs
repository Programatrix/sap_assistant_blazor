namespace SAPAssistant.Models
{
    public class DashboardCardModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = "";
        public string Value { get; set; } = "";
        public string Description { get; set; } = "";
        public bool IsFixed { get; set; } = false; // True = KPI base, False = KPI IA
        //public string TypeLabel => IsFixed ? "Fijo" : "IA";
        //public string TypeIcon => IsFixed ? "🔒" : "✨";
        public string TypeLabel { get; set; } = "";
        public string TypeIcon { get; set; } = "";
        public bool IsLoading { get; set; } = false; // 🔥 NUEVO: Controla si está en estado "cargando"
        public Func<Func<Task>, Task>? RefreshAsync { get; set; } // Para refrescar su valor
        public DashboardCardType CardType { get; set; } = DashboardCardType.ValueOnly;
        public double? Variation { get; set; }
        public List<double>? ChartData { get; set; } // para el mini gráfico
    }

    public enum DashboardCardType
    {
        ValueOnly,     // 🔥 Tarjeta simple: solo el valor principal
        Comparative,   // 🔥 Tarjeta con valor + porcentaje de variación
        MiniChart      // 🔥 Tarjeta con valor + mini gráfico de tendencia
    }
}
