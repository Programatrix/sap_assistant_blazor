namespace SAPAssistant.Models
{
    public class DashboardCardModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = "";
        public string Value { get; set; } = "";
        public string Description { get; set; } = "";
        public bool IsFixed { get; set; } = false; // True = KPI base, False = KPI IA

        public string TypeLabel { get; set; } = "";
        public string TypeIcon { get; set; } = "";
        public bool IsLoading { get; set; } = false; // 🔥 NUEVO: Controla si está en estado "cargando"

        public Func<Func<Task>, Task>? RefreshAsync { get; set; } // Para refrescar su valor
        public DashboardCardType CardType { get; set; } = DashboardCardType.ValueOnly;
        public double? Variation { get; set; }
        public List<double>? ChartData { get; set; } // para el mini gráfico

        // 🔽 Nuevos para el catálogo y vista detalle
        public string SqlQuery { get; set; } = "";
        public string SuggestedChart { get; set; } = "";
        public string[] DrillDownLevels { get; set; } = Array.Empty<string>();
        public string PromptOrigin { get; set; } = "";
        public string Category { get; set; } = "";

        // ✅ Nuevos metadatos informativos
        public string Unit { get; set; } = "";                   // €, %, unidades, etc.
        public DateTime? LastUpdated { get; set; }               // Fecha de última actualización
        public int Popularity { get; set; }                      // Cantidad de usuarios que lo usan
    }

    public enum DashboardCardType
    {
        ValueOnly,     // 🔥 Tarjeta simple: solo el valor principal
        Comparative,   // 🔥 Tarjeta con valor + porcentaje de variación
        MiniChart      // 🔥 Tarjeta con valor + mini gráfico de tendencia
    }
}
