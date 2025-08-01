using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SAPAssistant.Components.Chat;
using SAPAssistant.Models;
using SAPAssistant.Models.Chat;
using SAPAssistant.Service;
using SAPAssistant.Service.Interfaces;
using System.Text.Json;

namespace SAPAssistant.ViewModels;

public partial class ChatViewModel : BaseViewModel
{
    private readonly IJSRuntime _js;
    private readonly IAssistantService _assistantService;
    private readonly ChatHistoryService _chatHistoryService;
    private readonly StateContainer _stateContainer;

    public ElementReference MessagesContainer { get; set; }

    private string? preferredViewType;
    private ChatSession? CurrentSession { get; set; }

    [ObservableProperty]
    private List<MessageBase> messages = new();

    [ObservableProperty]
    private string userInput = string.Empty;

    [ObservableProperty]
    private bool isProcessing;

    public ChatViewModel(IJSRuntime js, IAssistantService assistantService, ChatHistoryService chatHistoryService, StateContainer stateContainer)
    {
        _js = js;
        _assistantService = assistantService;
        _chatHistoryService = chatHistoryService;
        _stateContainer = stateContainer;
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
            CurrentSession = await _chatHistoryService.GetChatSessionAsync(chatId);
        }
        else
        {
            CurrentSession = new ChatSession
            {
                Id = Guid.NewGuid().ToString(),
                Fecha = DateTime.Now,
                Titulo = "Nuevo chat"
            };
        }

        if (CurrentSession?.MensajesRaw != null)
        {
            foreach (var msgRaw in CurrentSession.MensajesRaw)
            {
                try
                {
                    var json = JsonSerializer.Serialize(msgRaw);
                    var tipo = msgRaw.TryGetValue("tipo", out var typeObj)
                               ? typeObj?.ToString()?.ToLower()
                               : null;

                    if (tipo == null) continue;

                    MessageBase? msg = tipo switch
                    {
                        "text" => JsonSerializer.Deserialize<TextMessage>(json, options),
                        "aclaracion" => JsonSerializer.Deserialize<TextMessage>(json, options),
                        "assistant" => JsonSerializer.Deserialize<TextMessage>(json, options),
                        "result" => JsonSerializer.Deserialize<ResultMessage>(json, options),
                        "error" => JsonSerializer.Deserialize<ErrorMessage>(json, options),
                        "system" => JsonSerializer.Deserialize<SystemMessage>(json, options),
                        "consulta" => JsonSerializer.Deserialize<ResultMessage>(json, options),
                        _ => null
                    };

                    if (msg != null)
                    {
                        if (msg is ResultMessage rm && !string.IsNullOrEmpty(preferredViewType))
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

    public async Task SendMessage(string message)
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
            var resultado = await _assistantService.ConsultarAsync(message, CurrentSession!.Id);

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
                _ => new ResultMessage
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
            var view = ((ResultMessage)msg).ViewType?.ToLower();
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

        if (msg is ResultMessage)
        {
            parameters["OnViewTypeChange"] = EventCallback.Factory.Create<string>(component, (string vt) => OnViewTypeChanged((ResultMessage)msg, vt));
        }

        return parameters;
    }

    public async Task OnViewTypeChanged(ResultMessage msg, string viewType)
    {
        msg.ViewType = viewType;
        preferredViewType = viewType;
        await _js.InvokeVoidAsync("viewTypePref.set", viewType);
    }
}

