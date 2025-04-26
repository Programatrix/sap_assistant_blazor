using SAPAssistant.Models;

namespace SAPAssistant.Service
{
    public class DashboardService
    {
        public List<DashboardCardModel> KPIs { get; set; } = new();

        public DashboardService()
        {
            InicializarDatosPrueba();
        }

        private void InicializarDatosPrueba()
        {
            // KPI 1: Valor Único (simple)
            KPIs.Add(new DashboardCardModel
            {
                Id = Guid.NewGuid(),
                Title = "Ventas Totales Hoy",
                Value = "$123,000",
                Description = "Total de ventas realizadas hoy.",
                CardType = DashboardCardType.ValueOnly, // 🔥 Tipo específico
                IsFixed = true,
                RefreshAsync = async (onFinished) =>
                {
                    await Task.Delay(500); // Simula refresco
                    if (onFinished != null)
                        await onFinished();
                }
            });

            // KPI 2: Comparativo (% variación)
            KPIs.Add(new DashboardCardModel
            {
                Id = Guid.NewGuid(),
                Title = "Ventas Hoy (Comparativo)",
                Value = "$123,000",
                Variation = 5.2, // 🔥 Ahora con variación positiva
                Description = "Ventas de hoy comparadas con ayer.",
                CardType = DashboardCardType.Comparative, // 🔥 Tipo Comparativa
                IsFixed = true,
                RefreshAsync = async (onFinished) =>
                {
                    await Task.Delay(500);
                    if (onFinished != null)
                        await onFinished();
                }
            });

            // KPI 3: Mini Gráfico
            KPIs.Add(new DashboardCardModel
            {
                Id = Guid.NewGuid(),
                Title = "Ventas Últimos 7 Días",
                Value = "$45,000",
                ChartData = new List<double> { 3000, 5000, 7000, 6000, 6500, 9000, 11000 }, // 🔥 Datos de ejemplo
                Description = "Tendencia de ventas de la semana.",
                CardType = DashboardCardType.MiniChart, // 🔥 Tipo MiniGráfico
                IsFixed = true,
                RefreshAsync = async (onFinished) =>
                {
                    await Task.Delay(500);
                    if (onFinished != null)
                        await onFinished();
                }
            });
        }

        public async Task RefreshAllKPIs()
        {
            foreach (var kpi in KPIs)
            {
                if (kpi.RefreshAsync != null)
                {
                    kpi.IsLoading = true;
                    await kpi.RefreshAsync(async () =>
                    {
                        kpi.IsLoading = false;
                    });
                }
            }
        }

        public void DeleteKPI(Guid id)
        {
            var kpi = KPIs.FirstOrDefault(k => k.Id == id);
            if (kpi != null && !kpi.IsFixed) // Solo dejar borrar dinámicos
            {
                KPIs.Remove(kpi);
            }
        }

        public async Task CreateNewKPI(string prompt, string chartType)
        {
            var newKpi = new DashboardCardModel
            {
                Id = Guid.NewGuid(),
                Title = prompt,
                Value = "🔄 Generando...",
                Description = $"KPI generado automáticamente ({chartType}).",
                IsFixed = false,
                IsLoading = true,
                CardType = DashboardCardType.ValueOnly, // 🔥 Por defecto, si quieres que luego varíe, puedes parametrizar
            };

            newKpi.RefreshAsync = async (onFinished) =>
            {
                await Task.Delay(2000);
                newKpi.Value = "$150,000";
                if (onFinished != null)
                    await onFinished();
            };

            KPIs.Add(newKpi);
        }
    }
}
