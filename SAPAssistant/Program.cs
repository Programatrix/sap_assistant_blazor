﻿using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Components.Web;
using SAPAssistant.Data;
using SAPAssistant.Security;
using SAPAssistant.Service;
using SAPAssistant.Security.Policies;
using Microsoft.AspNetCore.Authorization;
using SAPAssistant.Exceptions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor()
 .AddCircuitOptions(options => { options.DetailedErrors = true; });
builder.Services.AddHttpClient();

// API principal
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("http://91.99.139.55:8000")
});

// Servicios API específicos
builder.Services.AddHttpClient<ConnectionService>(client =>
{
    client.BaseAddress = new Uri("http://91.99.139.55:8000");
});

builder.Services.AddHttpClient<AssistantService>(client =>
{
    client.BaseAddress = new Uri("http://91.99.139.55:8000");
});


builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ProtectedSessionStorage>();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddSingleton<DashboardService>();
builder.Services.AddSingleton<NotificationService>();
builder.Services.AddScoped<ChatHistoryService>();


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
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
