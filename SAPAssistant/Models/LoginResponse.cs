﻿namespace SAPAssistant.Models
{
    public class LoginResponse
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<string> Permissions { get; set; } = new();
        public string Token { get; set; } = string.Empty;
        public string remote_url { get; set; } // ✅ Agregado
    }
    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

}
