using Microsoft.Extensions.Logging;
using SAPAssistant.Models;
using SAPAssistant.Models.Chat;
using SAPAssistant.Service.Interfaces;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Localization;
using SAPAssistant;
using SAPAssistant.Constants;
using System.Net.Http.Headers;
using SAPAssistant.Exceptions;

namespace SAPAssistant.Service
{
    public class ChatHistoryService : IChatHistoryService
    {
        private readonly HttpClient _http;
        private readonly SessionContextService _sessionContext;
        private readonly ILogger<ChatHistoryService> _logger;
        private readonly IStringLocalizer<ErrorMessages> _localizer;

        public ChatHistoryService(HttpClient http, SessionContextService sessionContext, ILogger<ChatHistoryService> logger, IStringLocalizer<ErrorMessages> localizer)
        {
            _http = http;
            _sessionContext = sessionContext;
            _logger = logger;
            _localizer = localizer;
        }

        public async Task<ServiceResult<List<ChatSession>>> GetChatHistoryAsync()
        {
            try
            {
                var token = await _sessionContext.GetTokenAsync();
                if (string.IsNullOrWhiteSpace(token))
                {
                    const string code = ErrorCodes.SESSION_TOKEN_NOT_FOUND;
                    return ServiceResult<List<ChatSession>>.Fail(_localizer[code], code);
                }

                var request = new HttpRequestMessage(HttpMethod.Get, "/assistant/chats");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await _http.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("[ChatService] Error HTTP {StatusCode}: {Error}", response.StatusCode, error);
                    const string code = ErrorCodes.CHAT_HISTORY_LOAD_ERROR;
                    return ServiceResult<List<ChatSession>>.Fail(_localizer[code], code);
                }

                var result = await response.Content.ReadFromJsonAsync<List<ChatSession>>() ?? new List<ChatSession>();
                return ServiceResult<List<ChatSession>>.Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ChatService] Excepción al obtener historial");
                const string code = ErrorCodes.CHAT_HISTORY_LOAD_ERROR;
                return ServiceResult<List<ChatSession>>.Fail(_localizer[code], code);
            }
        }

        public async Task<ServiceResult<ChatSession>> GetChatSessionAsync(string chatId)
        {
            try
            {
                var token = await _sessionContext.GetTokenAsync();
                if (string.IsNullOrWhiteSpace(token))
                {
                    const string code = ErrorCodes.SESSION_TOKEN_NOT_FOUND;
                    return ServiceResult<ChatSession>.Fail(_localizer[code], code);
                }

                var request = new HttpRequestMessage(HttpMethod.Get, $"assistant/chats/{chatId}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _http.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    _logger.LogError("[ChatService] Error HTTP {StatusCode}: {Error}", response.StatusCode, errorMessage);
                    const string code = ErrorCodes.CHAT_FETCH_ERROR;
                    return ServiceResult<ChatSession>.Fail(_localizer[code], code);
                }

                var chat = await response.Content.ReadFromJsonAsync<ChatSession>();
                if (chat == null)
                {
                    const string code = ErrorCodes.EMPTY_RESPONSE;
                    return ServiceResult<ChatSession>.Fail(_localizer[code], code);
                }

                var ok = ServiceResult<ChatSession>.Ok(chat, _localizer[ErrorCodes.CHAT_FETCH_SUCCESS]);
                ok.ErrorCode = ErrorCodes.CHAT_FETCH_SUCCESS;
                return ok;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "[ChatService] Error de red al obtener el chat '{ChatId}'", chatId);
                const string code = ErrorCodes.NET_ERROR;
                return ServiceResult<ChatSession>.Fail(_localizer[code], code);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ChatService] Excepción inesperada al obtener el chat '{ChatId}'", chatId);
                const string code = ErrorCodes.UNEXPECTED_ERROR;
                return ServiceResult<ChatSession>.Fail(_localizer[code], code);
            }
        }

        public async Task<ServiceResult> SaveChatSessionAsync(ChatSession session, List<MessageBase> mensajes)
        {
            try
            {
                // Serializar mensajes a diccionarios planos para MensajesRaw
                var json = JsonSerializer.Serialize(mensajes);
                session.MensajesRaw = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(json)!;

                var token = await _sessionContext.GetTokenAsync();
                if (string.IsNullOrWhiteSpace(token))
                {
                    const string code = ErrorCodes.SESSION_TOKEN_NOT_FOUND;
                    return ServiceResult.Fail(_localizer[code], code);
                }

                var request = new HttpRequestMessage(HttpMethod.Post, "/user-chats");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                request.Content = JsonContent.Create(session);

                var response = await _http.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("[ChatService] Error HTTP {StatusCode}: {Error}", response.StatusCode, error);
                    const string code = ErrorCodes.CHAT_SAVE_ERROR;
                    return ServiceResult.Fail(_localizer[code], code);
                }

                return ServiceResult.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ChatService] Excepción al guardar chat");
                const string code = ErrorCodes.CHAT_SAVE_ERROR;
                return ServiceResult.Fail(_localizer[code], code);
            }
        }

        public async Task<ServiceResult> DeleteChatSessionAsync(string chatId)
        {
            try
            {
                var token = await _sessionContext.GetTokenAsync();
                if (string.IsNullOrWhiteSpace(token))
                {
                    const string code = ErrorCodes.SESSION_TOKEN_NOT_FOUND;
                    return ServiceResult.Fail(_localizer[code], code);
                }

                var request = new HttpRequestMessage(HttpMethod.Delete, $"/user-chats/{chatId}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _http.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("[ChatService] Error HTTP {StatusCode}: {Error}", response.StatusCode, error);
                    const string code = ErrorCodes.CHAT_DELETE_ERROR;
                    return ServiceResult.Fail(_localizer[code], code);
                }

                return ServiceResult.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ChatService] Excepción al eliminar chat");
                const string code = ErrorCodes.CHAT_DELETE_ERROR;
                return ServiceResult.Fail(_localizer[code], code);
            }
        }

        public async Task<ServiceResult<ChatSession>> GetLastChatSessionAsync()
        {
            var history = await GetChatHistoryAsync();
            if (!history.Success)
            {
                return ServiceResult<ChatSession>.Fail(history.Message, history.ErrorCode);
            }

            var last = history.Data?.OrderByDescending(s => s.Fecha).FirstOrDefault();
            if (last == null)
            {
                const string code = ErrorCodes.EMPTY_RESPONSE;
                return ServiceResult<ChatSession>.Fail(_localizer[code], code);
            }

            return ServiceResult<ChatSession>.Ok(last);
        }
    }
}
