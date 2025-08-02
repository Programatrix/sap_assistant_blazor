using Microsoft.AspNetCore.Authorization;
using SAPAssistant.Service;
using SAPAssistant.Service.Interfaces;
using System.Security.Claims;

namespace SAPAssistant.Security.Policies
{
    public class ConnectionAuthorizationHandler : AuthorizationHandler<ConnectionRequirement>
    {
        private readonly SessionContextService _sessionContext;
        private readonly IConnectionService _connectionService;

        public ConnectionAuthorizationHandler(
            SessionContextService sessionContext,
            IConnectionService connectionService)
        {
            _sessionContext = sessionContext;
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

            // Intentar obtener conexión activa desde SessionContext
            var activeId = await _sessionContext.GetActiveConnectionIdAsync();
            if (string.IsNullOrWhiteSpace(activeId))
                return;

            // Validar conexión con el backend
            var isValid = await _connectionService.ValidateConnectionAsync(activeId);
            if (isValid)
            {
                context.Succeed(requirement);
            }
        }
    }
}
