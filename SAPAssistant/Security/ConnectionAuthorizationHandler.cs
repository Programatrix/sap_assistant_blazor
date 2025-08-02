using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using SAPAssistant.Service;
using SAPAssistant.Service.Interfaces;
using System.Security.Claims;

namespace SAPAssistant.Security.Policies
{
    public class ConnectionAuthorizationHandler : AuthorizationHandler<ConnectionRequirement>
    {
        private readonly ProtectedSessionStorage _sessionStorage;
        private readonly IConnectionService _connectionService;

        public ConnectionAuthorizationHandler(
            ProtectedSessionStorage sessionStorage,
            IConnectionService connectionService)
        {
            _sessionStorage = sessionStorage;
            _connectionService = connectionService;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            ConnectionRequirement requirement)
        {
            // Verifica si el usuario está autenticado
            var user = context.User;
            if (user?.Identity?.IsAuthenticated != true)
                return;

            // Intentar obtener conexión activa desde SessionStorage
            var connResult = await _sessionStorage.GetAsync<string>("active_connection_id");
            if (!connResult.Success || string.IsNullOrWhiteSpace(connResult.Value))
                return;

            // Validar conexión con el backend
            var isValid = await _connectionService.ValidateConnectionAsync(connResult.Value);
            if (isValid)
            {
                context.Succeed(requirement);
            }
        }
    }
}
