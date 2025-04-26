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
            // KPIs Fijos
            KPIs.Add(new DashboardCardModel
            {
                Id = Guid.NewGuid(),
                Title = "Ventas Totales Hoy",
                Value = "$123,000",
                Description = "Total de ventas realizadas hoy.",
                IsFixed = true,
                RefreshAsync = async (onFinished) =>
                {
                    await Task.Delay(500); // Simula refresco
                                           // Aquí podrías actualizar el valor real

                    if (onFinished != null)
                        await onFinished(); // 🔥 Notificar que terminó
                }

            });

            KPIs.Add(new DashboardCardModel
            {
                Id = Guid.NewGuid(),
                Title = "Pedidos Pendientes",
                Value = "24 pedidos",
                Description = "Órdenes de clientes aún sin despachar.",
                IsFixed = true,
                RefreshAsync = async (onFinished) =>
                {
                    await Task.Delay(500); // Simula refresco
                                           // Aquí podrías actualizar el valor real

                    if (onFinished != null)
                        await onFinished(); // 🔥 Notificar que terminó
                }
            });

            KPIs.Add(new DashboardCardModel
            {
                Id = Guid.NewGuid(),
                Title = "Stock Crítico",
                Value = "15 productos",
                Description = "Artículos con niveles bajos de inventario.",
                IsFixed = true,
                RefreshAsync = async (onFinished) =>
                {
                    await Task.Delay(500); // Simula refresco
                                           // Aquí podrías actualizar el valor real

                    if (onFinished != null)
                        await onFinished(); // 🔥 Notificar que terminó
                }
            });
        }

        public async Task RefreshAllKPIs()
        {
            foreach (var kpi in KPIs)
            {
                if (kpi.RefreshAsync != null)
                {
                    kpi.IsLoading = true; // 🔥 Opcional: marcar todos como cargando
                    await kpi.RefreshAsync(async () =>
                    {
                        kpi.IsLoading = false; // 🔥 Terminado el loading de cada KPI
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
                IsLoading = true, // Empieza cargando
            };

            newKpi.RefreshAsync = async (onFinished) =>
            {
                await Task.Delay(2000); // Simular proceso IA o SQL

                newKpi.Value = "$150,000"; // Actualizar valor

                if (onFinished != null)
                    await onFinished(); // 🔥 Llamar al callback para notificar fin
            };

            KPIs.Add(newKpi);
        }

    }

}
