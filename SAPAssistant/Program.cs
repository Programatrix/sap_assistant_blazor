using Microsoft.AspNetCore.Components;
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
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using SAPAssistant.Models;
using SAPAssistant.Constants;

var builder = WebApplication.CreateBuilder(args);

var apiBaseUrl = builder.Configuration["ApiBaseUrl"]
                 ?? Environment.GetEnvironmentVariable("API_BASE_URL")
                 ?? "http://127.0.0.1:8081";

// Services
builder.Services.AddRazorPages(); // ← necesario para la Razor Page de login
builder.Services.AddServerSideBlazor()
    .AddCircuitOptions(o => o.DetailedErrors = true);

builder.Services.AddLocalization(o => o.ResourcesPath = "");

// 🔐 Autenticación por cookies (config dev/prod)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "sapassistant.auth";
        options.Cookie.HttpOnly = true;

        // En dev permite HTTP; en prod exige HTTPS
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;

        // Buen default para mismo origen
        options.Cookie.SameSite = SameSiteMode.Lax;

        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromDays(7);

        // ⬇️ la página Razor de login
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/denied";
    });

// 🔐 Autorización (UNA sola vez) + tu política
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Conectado", policy =>
        policy.Requirements.Add(new ConnectionRequirement()));
});

// Antiforgery
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.Name = "XSRF-TOKEN";
    options.Cookie.HttpOnly = false;
    options.HeaderName = "X-CSRF-TOKEN";
});

// === HttpContext & Auth services ===
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<CurrentUserAccessor>();   // actualizado para usar HttpContext primero
builder.Services.AddScoped<SessionContextService>();

// === HttpClients ===
// Cliente base sin handler (lo usa AuthService para /auth/refresh)
builder.Services.AddHttpClient("Default", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

// Handler que adjunta Bearer y refresca en 401
builder.Services.AddTransient<AuthHandler>();

// Clientes de la API que deben enviar Bearer y refrescar
builder.Services.AddHttpClient<IConnectionService, ConnectionService>(client =>
{
    client.BaseAddress = new Uri($"{apiBaseUrl.TrimEnd('/')}/api/v1/");
}).AddHttpMessageHandler<AuthHandler>();

builder.Services.AddHttpClient<IAssistantService, AssistantService>(client =>
{
    client.BaseAddress = new Uri($"{apiBaseUrl.TrimEnd('/')}/api/v1/");
}).AddHttpMessageHandler<AuthHandler>();

builder.Services.AddHttpClient<ApiClient>(client =>
{
    client.BaseAddress = new Uri($"{apiBaseUrl.TrimEnd('/')}/api/v1/");
}).AddHttpMessageHandler<AuthHandler>();

// App services extra
builder.Services.AddScoped<DashboardService>();
builder.Services.AddSingleton<KpiCatalogService>();
builder.Services.AddScoped<IUserDashboardService, UserDashboardService>();
builder.Services.AddSingleton<INotificationService, NotificationService>();
builder.Services.AddScoped<IChatHistoryService, ChatHistoryService>();
builder.Services.Configure<CspOptions>(builder.Configuration.GetSection("Csp"));
builder.Services.AddSingleton<ICspBuilder, CspBuilder>();
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

// Handlers de autorización
builder.Services.AddScoped<IAuthorizationHandler, ConnectionAuthorizationHandler>();

var app = builder.Build();
var cspBuilder = app.Services.GetRequiredService<ICspBuilder>();

// Pipeline
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

// Cabeceras de seguridad
app.Use(async (context, next) =>
{
    context.Response.Headers["Content-Security-Policy"] = cspBuilder.Build();
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    await next();
});

// ⚠️ Orden correcto:
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Endpoints
app.MapPost("/auth/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.NoContent();
});

// Mapea Razor Pages (para /Account/Login)
app.MapRazorPages();

// Blazor
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
