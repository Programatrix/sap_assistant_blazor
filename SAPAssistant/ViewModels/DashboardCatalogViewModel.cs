using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Components;
using SAPAssistant.Models;
using SAPAssistant.Service;
using SAPAssistant.Service.Interfaces;
using SAPAssistant.Exceptions;

namespace SAPAssistant.ViewModels;

public partial class DashboardCatalogViewModel : BaseViewModel
{
    private readonly KpiCatalogService _kpiCatalogService;
    private readonly IUserDashboardService _userDashboardService;
    public DashboardService DashboardService { get; }

    [ObservableProperty]
    private List<DashboardCardModel>? catalog;

    [ObservableProperty]
    private string searchTerm = string.Empty;

    [ObservableProperty]
    private bool showFijos = true;

    [ObservableProperty]
    private bool showIA = true;

    [ObservableProperty]
    private HashSet<string> categoriasActivas = new();

    public IEnumerable<string> CategoriasUnicas => (Catalog ?? Enumerable.Empty<DashboardCardModel>())
        .Select(c => c.Category)
        .Where(c => !string.IsNullOrWhiteSpace(c))
        .Distinct();

    public IEnumerable<DashboardCardModel> CatalogFiltrado => (Catalog ?? Enumerable.Empty<DashboardCardModel>())
        .Where(k => FiltroTexto(k) && FiltroTipo(k) && FiltroCategoria(k));

    public DashboardCatalogViewModel(
        KpiCatalogService kpiCatalogService,
        IUserDashboardService userDashboardService,
        DashboardService dashboardService,
        INotificationService notificationService) : base(notificationService)
    {
        _kpiCatalogService = kpiCatalogService;
        _userDashboardService = userDashboardService;
        DashboardService = dashboardService;
    }

    public async Task InitializeAsync()
    {
        Catalog = await _kpiCatalogService.LoadCatalogAsync();
        CategoriasActivas = Catalog
            .Select(k => k.Category)
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .ToHashSet();
    }

    private bool FiltroTexto(DashboardCardModel kpi)
    {
        if (string.IsNullOrWhiteSpace(SearchTerm)) return true;
        var txt = SearchTerm.ToLower();
        return (kpi.Title?.ToLower().Contains(txt) ?? false)
            || (kpi.Description?.ToLower().Contains(txt) ?? false)
            || (kpi.Category?.ToLower().Contains(txt) ?? false);
    }

    private bool FiltroTipo(DashboardCardModel kpi)
    {
        return (kpi.IsFixed && ShowFijos) || (!kpi.IsFixed && ShowIA);
    }

    private bool FiltroCategoria(DashboardCardModel kpi)
    {
        return string.IsNullOrWhiteSpace(kpi.Category) || CategoriasActivas.Contains(kpi.Category);
    }

    public void ToggleCategoria(ChangeEventArgs e)
    {
        var cat = e.Value?.ToString();
        if (string.IsNullOrEmpty(cat)) return;

        if (CategoriasActivas.Contains(cat))
            CategoriasActivas.Remove(cat);
        else
            CategoriasActivas.Add(cat);
    }

    public async Task AddToDashboard(DashboardCardModel card)
    {
        var copy = new DashboardCardModel
        {
            Id = Guid.NewGuid(),
            Title = card.Title,
            Value = card.Value,
            Description = card.Description,
            IsFixed = card.IsFixed,
            TypeLabel = card.TypeLabel,
            TypeIcon = card.TypeIcon,
            CardType = card.CardType,
            Variation = card.Variation,
            ChartData = card.ChartData != null ? new List<double>(card.ChartData) : null,
            SqlQuery = card.SqlQuery,
            SuggestedChart = card.SuggestedChart,
            DrillDownLevels = card.DrillDownLevels.ToArray(),
            PromptOrigin = card.PromptOrigin,
            Category = card.Category
        };

        DashboardService.KPIs.Add(copy);
        var result = await _userDashboardService.AddKpiAsync(copy);
        if (!result.Success)
        {
            await NotificationService.Notify(result);
        }
    }
}
