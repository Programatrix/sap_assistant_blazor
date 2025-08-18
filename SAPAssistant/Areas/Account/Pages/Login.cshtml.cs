using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SAPAssistant.Constants;   // ErrorCodes
using SAPAssistant.Models;     // LoginRequest, LoginResponse
using SAPAssistant.Service;    // ApiClient

namespace SAPAssistant.Areas.Account.Pages
{
    public class LoginModel : PageModel
    {
        private readonly ApiClient _api;

        public LoginModel(ApiClient api) => _api = api;

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? ReturnUrl { get; set; }

        public void OnGet(string? returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null, CancellationToken ct = default)
        {
            returnUrl ??= Url.Content("~/");

            if (!ModelState.IsValid)
            {
                // Errores de validación de modelo (requeridos, etc.)
                ViewData["ToastMessage"] = "Revisa los campos obligatorios.";
                ViewData["ToastType"] = "warning";
                ViewData["ToastCode"] = "LOGIN_VALIDATION";
                return Page();
            }

            // 1) Validar credenciales contra tu API externa
            var req = new LoginRequest
            {
                Username = Input.Username,
                Password = Input.Password,
                RememberMe = Input.RememberMe
            };

            var result = await _api.PostAsResultAsync<LoginRequest, LoginResponse>(
                "login", req, okKey: ErrorCodes.LOGIN_SUCCESS, ct);

            if (!result.Success || result.Data is null)
            {
                // Mensaje para el ValidationSummary y toast
                var msg = string.IsNullOrWhiteSpace(result.Message)
                          ? "Usuario o contraseña incorrectos."
                          : result.Message;

                if (!string.IsNullOrWhiteSpace(result.TraceId))
                    msg = $"{msg} (trace: {result.TraceId})";

                ModelState.AddModelError(string.Empty, msg);

                ViewData["ToastMessage"] = msg;
                ViewData["ToastType"] = "error";
                ViewData["ToastCode"] = "LOGIN_INVALID";

                return Page();
            }

            // 2) Claims mínimos (añade roles si tu API los devuelve)
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, result.Data.Username),
                new(ClaimTypes.Name, result.Data.Username),
                // Si necesitas el token en el SERVIDOR:
                new("access_token", result.Data.Token),
                new("refresh_token", result.Data.refresh_token),
                new("remote_url", result.Data.remote_url ?? "")

        };
            // Ejemplo si tu API devuelve roles:
            // if (result.Data.Roles is { Length: >0 })
            //     claims.AddRange(result.Data.Roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            // 3) Cookie auth
            var props = new AuthenticationProperties
            {
                IsPersistent = Input.RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(Input.RememberMe ? 7 : 1)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                props);

            // (Opcional) Toast de bienvenida tras redirección:
            // TempData["ToastMessage"] = $"¡Bienvenido, {result.Data.Username}!";
            // TempData["ToastType"]    = "success";
            // TempData["ToastCode"]    = "LOGIN_OK";

            // 4) Redirigir
            return LocalRedirect(returnUrl);
        }

        public sealed class InputModel
        {
            [Required, Display(Name = "Usuario")]
            public string Username { get; set; } = "";

            [Required, DataType(DataType.Password), Display(Name = "Contraseña")]
            public string Password { get; set; } = "";

            [Display(Name = "Recordarme")]
            public bool RememberMe { get; set; } = true;
        }
    }
}
