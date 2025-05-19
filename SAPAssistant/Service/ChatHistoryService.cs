using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using SAPAssistant.Models;

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
            var request = new HttpRequestMessage(HttpMethod.Get, "/user-chats");
            request.Headers.Add("X-User-Id", userId);

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<ChatSession>>() ?? new List<ChatSession>();
        }

        public async Task<ChatSession?> GetChatSessionAsync(string chatId)
        {
            var userId = await GetUserIdAsync();
            var request = new HttpRequestMessage(HttpMethod.Get, $"/user-chats/{chatId}");
            request.Headers.Add("X-User-Id", userId);

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<ChatSession>();
        }

        public async Task SaveChatSessionAsync(ChatSession session)
        {
            var userId = await GetUserIdAsync();
            var request = new HttpRequestMessage(HttpMethod.Post, "/user-chats");
            request.Headers.Add("X-User-Id", userId);
            request.Content = JsonContent.Create(session);

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteChatSessionAsync(string chatId)
        {
            var userId = await GetUserIdAsync();
            var request = new HttpRequestMessage(HttpMethod.Delete, $"/user-chats/{chatId}");
            request.Headers.Add("X-User-Id", userId);

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }
    }

}
