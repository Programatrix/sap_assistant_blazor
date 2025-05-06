using SAPAssistant.Models;
using System.Text.Json;

namespace SAPAssistant.Service
    {
        public class KpiCatalogService
        {
            private readonly IWebHostEnvironment _env;

            public KpiCatalogService(IWebHostEnvironment env)
            {
                _env = env;
            }

            public async Task<List<DashboardCardModel>> LoadCatalogAsync()
            {
                var path = Path.Combine(_env.WebRootPath, "data/kpis_catalog.json");

                if (!File.Exists(path))
                    return new List<DashboardCardModel>();

                using var stream = File.OpenRead(path);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                return await JsonSerializer.DeserializeAsync<List<DashboardCardModel>>(stream, options)
                       ?? new List<DashboardCardModel>();
            }

            public async Task<DashboardCardModel?> GetByIdAsync(Guid id)
            {
                var catalog = await LoadCatalogAsync();
                return catalog.FirstOrDefault(k => k.Id == id);
            }

            public async Task<List<DashboardCardModel>> GetByCategoryAsync(string category)
            {
                var catalog = await LoadCatalogAsync();
                return catalog.Where(k => k.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
            }
        }
    }