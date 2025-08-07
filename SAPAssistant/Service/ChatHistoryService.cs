using Microsoft.Extensions.Logging;
using SAPAssistant.Exceptions;
using SAPAssistant.Models;
using SAPAssistant.Models.Chat;
using SAPAssistant.Service.Interfaces;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Localization;
using SAPAssistant;

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

        private async Task<string> GetUserIdAsync()
        {
            var userId = await _sessionContext.GetUserIdAsync();
            if (string.IsNullOrEmpty(userId)) throw new ChatHistoryServiceException(_localizer["SESSION-USER-NOT-FOUND"]);
            return userId;
        }

        public async Task<List<ChatSession>> GetChatHistoryAsync()
        {
            var userId = await GetUserIdAsync();

            var request = new HttpRequestMessage(HttpMethod.Get, "/assistant/chats");
            request.Headers.Add("X-User-Id", userId);

            var response = await _http.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new ChatHistoryServiceException($"{_localizer["CHAT-HISTORY-LOAD-ERROR"]}: {response.StatusCode} - {error}");
            }
            List<ChatSession> result = null;
            try
            {
                result = await response.Content.ReadFromJsonAsync<List<ChatSession>>() ?? new List<ChatSession>();
            }
            catch (Exception)
            {
                throw;
            }
            return result;
        }

        public async Task<OperationResult<ChatSession>> GetChatSessionAsync(string chatId)
        {
            try
            {
                var userId = await GetUserIdAsync();

                var request = new HttpRequestMessage(HttpMethod.Get, $"assistant/chats/{chatId}");
                request.Headers.Add("X-User-Id", userId);

                var response = await _http.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    _logger.LogError("[ChatService] Error HTTP {StatusCode}: {Error}", response.StatusCode, errorMessage);
                    const string code = "CHAT-FETCH-ERROR";
                    return OperationResult<ChatSession>.Fail(_localizer[code], code);
                }

                var chat = await response.Content.ReadFromJsonAsync<ChatSession>();
                if (chat == null)
                {
                    const string code = "EMPTY-RESPONSE";
                    return OperationResult<ChatSession>.Fail(_localizer[code], code);
                }

                var ok = OperationResult<ChatSession>.Ok(chat, _localizer["CHAT-FETCH-SUCCESS"]);
                ok.ErrorCode = "CHAT-FETCH-SUCCESS";
                return ok;
            }
            catch (HttpRequestException)
            {
                const string code = "NET-ERROR";
                return OperationResult<ChatSession>.Fail(_localizer[code], code);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ChatService] Excepción inesperada al obtener el chat '{ChatId}'", chatId);
                const string code = "UNEXPECTED-ERROR";
                return OperationResult<ChatSession>.Fail(_localizer[code], code);
            }
        }

        public async Task SaveChatSessionAsync(ChatSession session, List<MessageBase> mensajes)
        {
            var userId = await GetUserIdAsync();

            // Serializar mensajes a diccionarios planos para MensajesRaw
            var json = JsonSerializer.Serialize(mensajes);
            session.MensajesRaw = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(json)!;

            var request = new HttpRequestMessage(HttpMethod.Post, "/user-chats");
            request.Headers.Add("X-User-Id", userId);
            request.Content = JsonContent.Create(session);

            var response = await _http.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new ChatHistoryServiceException($"{_localizer["CHAT-SAVE-ERROR"]}: {response.StatusCode} - {error}");
            }
        }

        public async Task DeleteChatSessionAsync(string chatId)
        {
            var userId = await GetUserIdAsync();

            var request = new HttpRequestMessage(HttpMethod.Delete, $"/user-chats/{chatId}");
            request.Headers.Add("X-User-Id", userId);

            var response = await _http.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new ChatHistoryServiceException($"{_localizer["CHAT-DELETE-ERROR"]}: {response.StatusCode} - {error}");
            }
        }

        public async Task<ChatSession?> GetLastChatSessionAsync()
        {
            var sessions = await GetChatHistoryAsync();
            return sessions.OrderByDescending(s => s.Fecha).FirstOrDefault();
        }
    }
}
