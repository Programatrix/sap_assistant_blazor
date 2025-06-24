using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using SAPAssistant.Models;
using SAPAssistant.Models.Chat;
using System.Net.Http.Json;
using System.Text.Json;

namespace SAPAssistant.Service
{
    public class ChatHistoryService
    {
        private readonly HttpClient _http;
        private readonly ProtectedSessionStorage _sessionStorage;

        public ChatHistoryService(HttpClient http, ProtectedSessionStorage sessionStorage)
        {
            _http = http;
            _sessionStorage = sessionStorage;
        }

        private async Task<string> GetUserIdAsync()
        {
            var userResult = await _sessionStorage.GetAsync<string>("username");
            if (!userResult.Success) throw new Exception("Usuario no encontrado en la sesión.");
            return userResult.Value!;
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
                throw new Exception($"Error al cargar historial: {response.StatusCode} - {error}");
            }
            List<ChatSession> result=null;
            try
            {             
                result = await response.Content.ReadFromJsonAsync<List<ChatSession>>() ?? new List<ChatSession>();

            }
            catch (Exception ex)
            {
                throw;
            }
            return result;
        }

        public async Task<ChatSession?> GetChatSessionAsync(string chatId)
        {
            try
            {
                var userId = await GetUserIdAsync();

                var request = new HttpRequestMessage(HttpMethod.Get, $"assistant/chats/{chatId}");
                request.Headers.Add("X-User-Id", userId);
                //request.Headers.Add("chat_id", chatId);

                var response = await _http.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    Console.Error.WriteLine($"[ChatService] Error HTTP {response.StatusCode}: {errorMessage}");
                    return null; // Retorna null en lugar de lanzar excepción
                }

                var chat = await response.Content.ReadFromJsonAsync<ChatSession>();
                return chat;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[ChatService] Excepción inesperada al obtener el chat '{chatId}': {ex.Message}");
                return null;
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
                throw new Exception($"Error al guardar chat: {response.StatusCode} - {error}");
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
                throw new Exception($"Error al eliminar chat: {response.StatusCode} - {error}");
            }
        }

        public async Task<ChatSession?> GetLastChatSessionAsync()
        {
            var sessions = await GetChatHistoryAsync();
            return sessions.OrderByDescending(s => s.Fecha).FirstOrDefault();
        }
    }
}
