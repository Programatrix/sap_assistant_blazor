using Microsoft.Extensions.Logging;
using SAPAssistant.Exceptions;
using SAPAssistant.Models;
using SAPAssistant.Models.Chat;
using SAPAssistant.Service.Interfaces;
using System.Net.Http.Json;
using System.Text.Json;

namespace SAPAssistant.Service
{
    public class ChatHistoryService : IChatHistoryService
    {
        private readonly HttpClient _http;
        private readonly SessionContextService _sessionContext;
        private readonly ILogger<ChatHistoryService> _logger;

        public ChatHistoryService(HttpClient http, SessionContextService sessionContext, ILogger<ChatHistoryService> logger)
        {
            _http = http;
            _sessionContext = sessionContext;
            _logger = logger;
        }

        private async Task<string> GetUserIdAsync()
        {
            var userId = await _sessionContext.GetUserIdAsync();
            if (string.IsNullOrEmpty(userId)) throw new ChatHistoryServiceException("Usuario no encontrado en la sesión.");
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
                throw new ChatHistoryServiceException($"Error al cargar historial: {response.StatusCode} - {error}");
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
                    return OperationResult<ChatSession>.Fail("Error al obtener el chat.", $"HTTP-{(int)response.StatusCode}");
                }

                var chat = await response.Content.ReadFromJsonAsync<ChatSession>();
                if (chat == null)
                    return OperationResult<ChatSession>.Fail("Respuesta vacía del servidor.", "EMPTY-RESPONSE");

                return OperationResult<ChatSession>.Ok(chat, "Chat obtenido exitosamente.");
            }
            catch (HttpRequestException)
            {
                return OperationResult<ChatSession>.Fail("Problema de conexión. Verifique su conexión a Internet.", "NET-ERROR");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ChatService] Excepción inesperada al obtener el chat '{ChatId}'", chatId);
                return OperationResult<ChatSession>.Fail("Error inesperado al obtener el chat.", "UNEXPECTED-ERROR");
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
                throw new ChatHistoryServiceException($"Error al guardar chat: {response.StatusCode} - {error}");
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
                throw new ChatHistoryServiceException($"Error al eliminar chat: {response.StatusCode} - {error}");
            }
        }

        public async Task<ChatSession?> GetLastChatSessionAsync()
        {
            var sessions = await GetChatHistoryAsync();
            return sessions.OrderByDescending(s => s.Fecha).FirstOrDefault();
        }
    }
}
