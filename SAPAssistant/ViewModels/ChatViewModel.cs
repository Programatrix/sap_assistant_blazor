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
    public ElementReference MessagesContainer { get; set; }

    private string? preferredViewType;
    private ChatSession? CurrentSession { get; set; }

    private HubConnection? _hubConnection;

    public string CurrentPhase { get; private set; } = string.Empty;
    public double? ProgressValue { get; private set; }
    public bool IsReconnecting { get; private set; }
    public Action? StateHasChanged { get; set; }

    [ObservableProperty]
    private List<MessageBase> messages = new();

    [ObservableProperty]
    private string userInput = string.Empty;

    [ObservableProperty]
    private bool isProcessing;

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
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        Messages.Clear();

        if (!string.IsNullOrWhiteSpace(chatId))
        {
            var result = await _chatHistoryService.GetChatSessionAsync(chatId);
            if (result.Success)
            {
                CurrentSession = result.Data;
            }
            else
            {
                await NotificationService.Notify(result);
                return;
            }
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
            foreach (var msgRaw in CurrentSession.Messages)
            {
                try
                {
                    var json = JsonSerializer.Serialize(msgRaw);
                    var tipo = msgRaw.Tipo;
                    if (tipo == null) continue;

                    MessageBase? msg = tipo switch
                    {
                        "text" => JsonSerializer.Deserialize<TextMessage>(json, options),
                        "aclaracion" => JsonSerializer.Deserialize<TextMessage>(json, options),
                        "assistant" => JsonSerializer.Deserialize<TextMessage>(json, options),
                        "result" => JsonSerializer.Deserialize<ChatResultMessage>(json, options),
                        "error" => JsonSerializer.Deserialize<ErrorMessage>(json, options),
                        "system" => JsonSerializer.Deserialize<SystemMessage>(json, options),
                        "consulta" => JsonSerializer.Deserialize<ChatResultMessage>(json, options),
                        _ => null
                    };

                    if (msg != null)
                    {
                        if (msg is ChatResultMessage rm && !string.IsNullOrEmpty(preferredViewType))
                        {
                            rm.ViewType = preferredViewType;
                        }
                        Messages.Add(msg);
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Error al deserializar mensaje: {ex.Message}");
                }
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
        IsReconnecting = false;

        var userMsg = new TextMessage
        {
            Mensaje = message,
            Role = "user"
        };

        Messages.Add(userMsg);
        await ScrollToBottom();

        if (isDemo)
        {
            var resultadoDemo = await _assistantService.ConsultarDemoAsync(message);
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

            IsProcessing = false;
            await ScrollToBottom();
            return;
        }

        var startResult = await _assistantService.StartQueryAsync(message, CurrentSession!.Id);
        if (!startResult.Success || string.IsNullOrWhiteSpace(startResult.Data))
        {
            Messages.Add(new ErrorMessage { Mensaje = startResult.Message });
            await NotificationService.Notify(startResult);
            IsProcessing = false;
            await ScrollToBottom();
            return;
        }

        var requestId = startResult.Data;

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

        _hubConnection.Reconnecting += error =>
        {
            IsReconnecting = true;
            StateHasChanged?.Invoke();
            return Task.CompletedTask;
        };

        _hubConnection.Reconnected += connectionId =>
        {
            IsReconnecting = false;
            StateHasChanged?.Invoke();
            return Task.CompletedTask;
        };

        _hubConnection.Closed += error =>
        {
            IsReconnecting = false;
            IsProcessing = false;
            StateHasChanged?.Invoke();
            return Task.CompletedTask;
        };

        await _hubConnection.StartAsync();
        await _hubConnection.InvokeAsync("Subscribe", requestId);

    }

    public async Task ScrollToBottom()
    {
        await _js.InvokeVoidAsync("chatEnhancer.scrollToBottom", MessagesContainer);
    }

    public Type GetComponentType(MessageBase msg)
    {
        if (msg.Type == "Result")
        {
            var view = ((ChatResultMessage)msg).ViewType?.ToLower();
            return view switch
            {
                "cards" => typeof(ResultCardListComponent),
                "kpi" => typeof(ResultKpiComponent),
                "chart" => typeof(ResultChartComponent),
                _ => typeof(ResultGridComponent)
            };
        }

        return msg.Type switch
        {
            "Text" => typeof(TextMessageComponent),
            "Error" => typeof(ErrorMessageComponent),
            "system" => typeof(SystemMessageComponent),
            _ => typeof(TextMessageComponent)
        };
    }

    public Dictionary<string, object> GetParameters(ComponentBase component, MessageBase msg)
    {
        var parameters = new Dictionary<string, object>
        {
            ["Message"] = msg
        };

        if (msg is ChatResultMessage)
        {
            parameters["OnViewTypeChange"] = EventCallback.Factory.Create<string>(component, (string vt) => OnViewTypeChanged((ChatResultMessage)msg, vt));
        }

        return parameters;
    }

    public async Task OnViewTypeChanged(ChatResultMessage msg, string viewType)
    {
        msg.ViewType = viewType;
        preferredViewType = viewType;
        await _js.InvokeVoidAsync("viewTypePref.set", viewType);
    }

    private async Task OnProgressUpdate(ProgressUpdate update)
    {
        CurrentPhase = update.Phase;
        ProgressValue = Math.Clamp(update.Percent / 100.0, 0, 1);

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
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var data = JsonSerializer.Deserialize<QueryResponse>(update.Message, options);
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
            catch (JsonException)
            {
                Messages.Add(new ErrorMessage { Mensaje = "Error al procesar progreso." });
            }
        }

        StateHasChanged?.Invoke();
        await ScrollToBottom();
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection != null)
        {
            try
            {
                await _hubConnection.StopAsync();
                await _hubConnection.DisposeAsync();
            }
            catch (Exception) { }
        }
    }
}
