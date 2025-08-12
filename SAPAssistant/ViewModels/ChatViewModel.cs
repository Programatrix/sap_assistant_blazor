using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SAPAssistant.Components.Chat;
using SAPAssistant.Models;
using SAPAssistant.Models.Chat;
using SAPAssistant.Service;
using SAPAssistant.Service.Interfaces;
using System.Text.Json;
using SAPAssistant.Exceptions;
using Microsoft.Extensions.Localization;
using SAPAssistant;

namespace SAPAssistant.ViewModels;

public partial class ChatViewModel : BaseViewModel
{
    private readonly IJSRuntime _js;
    private readonly IAssistantService _assistantService;
    private readonly IChatHistoryService _chatHistoryService;
    private readonly StateContainer _stateContainer;
    private readonly IStringLocalizer<ErrorMessages> _localizer;

    public ElementReference MessagesContainer { get; set; }

    private string? preferredViewType;
    private ChatSession? CurrentSession { get; set; }

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
        IStringLocalizer<ErrorMessages> localizer) : base(notificationService)
    {
        _js = js;
        _assistantService = assistantService;
        _chatHistoryService = chatHistoryService;
        _stateContainer = stateContainer;
        _localizer = localizer;
    }

    public async Task OnInitializedAsync()
    {
        preferredViewType = await _js.InvokeAsync<string>("viewTypePref.get");
    }

    public async Task OnParametersSetAsync(string? chatId)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

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
                Titulo = _localizer["NEW-CHAT-TITLE"]
            };
        }

        if (CurrentSession?.Messages  != null)
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

        var userMsg = new TextMessage
        {
            Mensaje = message,
            Role = "user"
        };

        Messages.Add(userMsg);

        try
        {            
            QueryResponse? resultado = isDemo
            ? await _assistantService.ConsultarDemoAsync(message)
            : await _assistantService.ConsultarAsync(message, CurrentSession!.Id);

            MessageBase responseMsg = resultado?.Tipo switch
            {
                "aclaracion" or "assistant" => new TextMessage
                {
                    Role = "assistant",
                    Mensaje = resultado.Mensaje
                },
                "resumen" => new TextMessage
                {
                    Mensaje = resultado.Resumen
                },
                "system" => new SystemMessage
                {
                    Mensaje = resultado.Mensaje
                },
                _ => new ChatResultMessage
                {
                    Resumen = resultado?.Resumen ?? "",
                    Sql = resultado?.Sql ?? "",
                    Data = resultado?.Resultados ?? new(),
                    ViewType = preferredViewType ?? resultado?.ViewType ?? "grid"
                }
            };

            Messages.Add(responseMsg);
        }
        catch (Exception ex)
        {
            Messages.Add(new ErrorMessage
            {
                Mensaje = ex.Message
            });
        }

        IsProcessing = false;
        await ScrollToBottom();
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
}

