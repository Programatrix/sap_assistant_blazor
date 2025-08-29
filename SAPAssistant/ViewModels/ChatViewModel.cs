// ChatViewModel.cs (actualizado con soporte para mensajes de progreso paso a paso)

using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.SignalR.Client;
using SAPAssistant.Components.Chat;
using SAPAssistant.Models;
using SAPAssistant.Models.Chat;
using SAPAssistant.Service;
using SAPAssistant.Service.Interfaces;
using System.Text.Json;
using SAPAssistant.Exceptions;
using Microsoft.Extensions.Localization;
using SAPAssistant;
using SAPAssistant.Constants;
using System.Net.Http;

namespace SAPAssistant.ViewModels;

public partial class ChatViewModel : BaseViewModel, IAsyncDisposable
{
    private readonly IJSRuntime _js;
    private readonly IAssistantService _assistantService;
    private readonly IChatHistoryService _chatHistoryService;
    private readonly StateContainer _stateContainer;
    private readonly IStringLocalizer<ErrorMessages> _localizer;
    private readonly HttpClient _http;
    private readonly NavigationManager _nav;

    private HubConnection? _hubConnection;
    private string? preferredViewType;
    private ChatSession? CurrentSession { get; set; }

    public ElementReference MessagesContainer { get; set; }
    public Action? StateHasChanged { get; set; }

    public string CurrentPhase { get; private set; } = string.Empty;
    public double? ProgressValue { get; private set; }
    public bool IsReconnecting { get; private set; }
    public List<string> ProgressMessages { get; private set; } = new();

    [ObservableProperty] private List<MessageBase> messages = new();
    [ObservableProperty] private string userInput = string.Empty;
    [ObservableProperty] private bool isProcessing;

    public ChatViewModel(
        IJSRuntime js,
        IAssistantService assistantService,
        IChatHistoryService chatHistoryService,
        StateContainer stateContainer,
        INotificationService notificationService,
        IStringLocalizer<ErrorMessages> localizer,
        HttpClient http,
        NavigationManager navigationManager) : base(notificationService)
    {
        _js = js;
        _assistantService = assistantService;
        _chatHistoryService = chatHistoryService;
        _stateContainer = stateContainer;
        _localizer = localizer;
        _http = http;
        _nav = navigationManager;
    }

    public async Task OnInitializedAsync()
    {
        preferredViewType = await _js.InvokeAsync<string>("viewTypePref.get");
    }

    public async Task OnParametersSetAsync(string? chatId)
    {
        Messages.Clear();

        if (!string.IsNullOrWhiteSpace(chatId))
        {
            var result = await _chatHistoryService.GetChatSessionAsync(chatId);
            if (!result.Success)
            {
                await NotificationService.Notify(result);
                return;
            }
            CurrentSession = result.Data;
        }
        else
        {
            CurrentSession = new ChatSession
            {
                Id = Guid.NewGuid().ToString(),
                Fecha = DateTime.Now,
                Titulo = _localizer[ErrorCodes.NEW_CHAT_TITLE]
            };
        }

        if (CurrentSession?.Messages != null)
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            foreach (var msgRaw in CurrentSession.Messages)
            {
                try
                {
                    var json = JsonSerializer.Serialize(msgRaw);
                    var tipo = msgRaw.Tipo;

                    MessageBase? msg = tipo switch
                    {
                        "text" or "aclaracion" or "assistant" => JsonSerializer.Deserialize<TextMessage>(json, options),
                        "result" or "consulta" => JsonSerializer.Deserialize<ChatResultMessage>(json, options),
                        "error" => JsonSerializer.Deserialize<ErrorMessage>(json, options),
                        "system" => JsonSerializer.Deserialize<SystemMessage>(json, options),
                        _ => null
                    };

                    if (msg is ChatResultMessage rm && !string.IsNullOrEmpty(preferredViewType))
                        rm.ViewType = preferredViewType;

                    if (msg != null)
                        Messages.Add(msg);
                }
                catch { }
            }
        }

        _stateContainer.CurrentChat = CurrentSession;
    }

    public async Task SendMessage(string message, bool isDemo)
    {
        if (string.IsNullOrWhiteSpace(message) || IsProcessing) return;

        IsProcessing = true;
        CurrentPhase = string.Empty;
        ProgressValue = null;
        ProgressMessages.Clear();
        IsReconnecting = false;

        Messages.Add(new TextMessage { Mensaje = message, Role = "user" });
        await ScrollToBottom();

        if (isDemo)
        {
            var resultadoDemo = await _assistantService.ConsultarDemoAsync(message);
            IsProcessing = false;

            if (!resultadoDemo.Success || resultadoDemo.Data == null)
            {
                Messages.Add(new ErrorMessage { Mensaje = resultadoDemo.Message });
                await NotificationService.Notify(resultadoDemo);
            }
            else
            {
                var data = resultadoDemo.Data;
                MessageBase responseMsg = data.Tipo switch
                {
                    "aclaracion" or "assistant" => new TextMessage { Role = "assistant", Mensaje = data.Mensaje },
                    "resumen" => new TextMessage { Mensaje = data.Resumen },
                    "system" => new SystemMessage { Mensaje = data.Mensaje },
                    _ => new ChatResultMessage
                    {
                        Resumen = data.Resumen ?? "",
                        Sql = data.Sql ?? "",
                        Data = data.Resultados ?? new(),
                        ViewType = preferredViewType ?? data.ViewType ?? "grid"
                    }
                };
                Messages.Add(responseMsg);
            }
            await ScrollToBottom();
            return;
        }

        var requestId = Guid.NewGuid().ToString();

        if (_hubConnection != null)
        {
            await _hubConnection.StopAsync();
            await _hubConnection.DisposeAsync();
        }

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(_nav.ToAbsoluteUri("/hubs/progress"))
            .WithAutomaticReconnect()
            .Build();

        _hubConnection.On<ProgressUpdate>("ProgressUpdate", update => OnProgressUpdate(update));

        _hubConnection.Reconnecting += error => { IsReconnecting = true; StateHasChanged?.Invoke(); return Task.CompletedTask; };
        _hubConnection.Reconnected += connectionId => { IsReconnecting = false; StateHasChanged?.Invoke(); return Task.CompletedTask; };
        _hubConnection.Closed += error => { IsReconnecting = false; IsProcessing = false; StateHasChanged?.Invoke(); return Task.CompletedTask; };

        await _hubConnection.StartAsync();
        await _hubConnection.InvokeAsync("Subscribe", requestId);

        var startResult = await _assistantService.StartQueryAsync(message, CurrentSession!.Id, requestId);
        if (!startResult.Success)
        {
            Messages.Add(new ErrorMessage { Mensaje = startResult.Message });
            await NotificationService.Notify(startResult);
            IsProcessing = false;
            await ScrollToBottom();
        }
    }

    private async Task OnProgressUpdate(ProgressUpdate update)
    {
        CurrentPhase = !string.IsNullOrWhiteSpace(update.Message)
            ? update.Message
            : GetDefaultMessage(update.Phase);

        ProgressValue = Math.Clamp(update.Percent / 100.0, 0, 1);

        if (!string.IsNullOrWhiteSpace(update.Message))
            ProgressMessages.Add(update.Message);

        if (update.Phase == "error")
        {
            IsProcessing = false;
            Messages.Add(new ErrorMessage { Mensaje = update.Message });
        }
        else if (update.Phase == "done")
        {
            IsProcessing = false;
            try
            {
                var data = JsonSerializer.Deserialize<QueryResponse>(update.Message, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (data != null)
                {
                    MessageBase responseMsg = data.Tipo switch
                    {
                        "aclaracion" or "assistant" => new TextMessage { Role = "assistant", Mensaje = data.Mensaje },
                        "resumen" => new TextMessage { Mensaje = data.Resumen },
                        "system" => new SystemMessage { Mensaje = data.Mensaje },
                        _ => new ChatResultMessage
                        {
                            Resumen = data.Resumen ?? "",
                            Sql = data.Sql ?? "",
                            Data = data.Resultados ?? new(),
                            ViewType = preferredViewType ?? data.ViewType ?? "grid"
                        }
                    };
                    Messages.Add(responseMsg);
                }
            }
            catch
            {
                Messages.Add(new ErrorMessage { Mensaje = "Error al procesar progreso." });
            }
        }

        StateHasChanged?.Invoke();
        await ScrollToBottom();
    }

    private string GetDefaultMessage(string? phase)
    {
        return phase?.ToLowerInvariant() switch
        {
            "start" => "🔄 Iniciando proceso...",
            "classification" => "🧠 Clasificando la consulta...",
            "validate_sql" => "🔍 Validando la SQL generada...",
            "generate_query" => "💾 Generando la consulta final...",
            "execute_sql" => "🚀 Ejecutando la consulta en SAP...",
            "format_result" => "📊 Formateando los resultados...",
            "done" => "✅ Consulta completada.",
            "error" => "❌ Ha ocurrido un error.",
            _ => $"⏳ Ejecutando etapa: {phase}..."
        };
    }

    public async Task ScrollToBottom() => await _js.InvokeVoidAsync("chatEnhancer.scrollToBottom", MessagesContainer);

    public Type GetComponentType(MessageBase msg) => msg switch
    {
        ChatResultMessage r => r.ViewType?.ToLower() switch
        {
            "cards" => typeof(ResultCardListComponent),
            "kpi" => typeof(ResultKpiComponent),
            "chart" => typeof(ResultChartComponent),
            _ => typeof(ResultGridComponent)
        },
        TextMessage => typeof(TextMessageComponent),
        ErrorMessage => typeof(ErrorMessageComponent),
        SystemMessage => typeof(SystemMessageComponent),
        _ => typeof(TextMessageComponent)
    };

    public Dictionary<string, object> GetParameters(ComponentBase component, MessageBase msg)
    {
        var parameters = new Dictionary<string, object> { ["Message"] = msg };
        if (msg is ChatResultMessage r)
            parameters["OnViewTypeChange"] = EventCallback.Factory.Create<string>(component, (vt) => OnViewTypeChanged(r, vt));
        return parameters;
    }

    public async Task OnViewTypeChanged(ChatResultMessage msg, string viewType)
    {
        msg.ViewType = viewType;
        preferredViewType = viewType;
        await _js.InvokeVoidAsync("viewTypePref.set", viewType);
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection != null)
        {
            try { await _hubConnection.StopAsync(); await _hubConnection.DisposeAsync(); } catch { }
        }
    }
}
