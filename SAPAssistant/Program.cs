using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using SAPAssistant.Security;
using SAPAssistant.Service;
using SAPAssistant.Service.Interfaces;
using SAPAssistant.Security.Policies;
using Microsoft.AspNetCore.Authorization;
using SAPAssistant.Exceptions;
using MudBlazor.Services;
using SAPAssistant.ViewModels;
using System.Globalization;
using Microsoft.AspNetCore.Localization;

// ⮕ si ApiClient está en tu proyecto, importa su namespace
// using YourNamespace.Infrastructure.Http;  // donde esté ApiClient

var builder = WebApplication.CreateBuilder(args);

var apiBaseUrl = builder.Configuration["ApiBaseUrl"]
                 ?? Environment.GetEnvironmentVariable("API_BASE_URL")
                 ?? "http://127.0.0.1:8081";

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor()
    .AddCircuitOptions(options => { options.DetailedErrors = true; });

builder.Services.AddLocalization(options => options.ResourcesPath = "");

// Cliente “Default” (si lo usas en otros sitios)
builder.Services.AddHttpClient("Default", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

// ❌ Eliminado el scoped genérico de HttpClient si ya no se usa directamente
// builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });

// Servicios API específicos → apuntan a /api/v1/
builder.Services.AddHttpClient<IConnectionService, ConnectionService>(client =>
{
    client.BaseAddress = new Uri($"{apiBaseUrl.TrimEnd('/')}/api/v1/");
});

builder.Services.AddHttpClient<IAssistantService, AssistantService>(client =>
{
    client.BaseAddress = new Uri($"{apiBaseUrl.TrimEnd('/')}/api/v1/");
});

// ✅ ApiClient (typed) con BaseAddress que incluye /api/v1/
builder.Services.AddHttpClient<ApiClient>(client =>
{
    client.BaseAddress = new Uri($"{apiBaseUrl.TrimEnd('/')}/api/v1/");
});
//builder.Services.AddScoped<ApiClient>();

// Resto de servicios de la app
builder.Services.AddScoped<AuthService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<SessionContextService>();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddSingleton<KpiCatalogService>();
builder.Services.AddScoped<IUserDashboardService, UserDashboardService>();
builder.Services.AddSingleton<INotificationService, NotificationService>();
builder.Services.AddScoped<IChatHistoryService, ChatHistoryService>();
builder.Services.AddMudServices();

builder.Services.AddScoped<StateContainer>();
builder.Services.AddScoped<ChatViewModel>();
builder.Services.AddScoped<ChatHistoryViewModel>();
builder.Services.AddScoped<HomeViewModel>();
builder.Services.AddScoped<LoginViewModel>();
builder.Services.AddScoped<ConnectionSettingsViewModel>();
builder.Services.AddScoped<ConnectionSelectionViewModel>();
builder.Services.AddScoped<ConnectionManagerViewModel>();
builder.Services.AddScoped<DashboardPageViewModel>();
builder.Services.AddScoped<DashboardCatalogViewModel>();
builder.Services.AddScoped<DashboardWizardViewModel>();

// ✅ Política de conexión activa
builder.Services.AddAuthorizationCore(options =>
{
    options.AddPolicy("Conectado", policy =>
        policy.Requirements.Add(new ConnectionRequirement()));
});

// ✅ Handler de autorización
builder.Services.AddScoped<IAuthorizationHandler, ConnectionAuthorizationHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

var supportedCultures = new[] { new CultureInfo("en"), new CultureInfo("es") };
var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new("es"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
};
app.UseRequestLocalization(localizationOptions);

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
