using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using QuestionForge.AppWeb.Models;

namespace QuestionForge.AppWeb.Controllers
{
    // Clase base para manejar autorización
    public class BaseController : Controller
    {
        private readonly string[] _skipAuthActions = { "Login", "Registrar" };

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var actionName = context.RouteData.Values["action"]?.ToString();

            if (!_skipAuthActions.Contains(actionName))
            {
                if (string.IsNullOrEmpty(context.HttpContext.Session.GetString("JwtToken")))
                {
                    context.Result = new RedirectToActionResult("Login", "Usuario", null);
                }
            }

            base.OnActionExecuting(context);
        }
    }

    public class UsuarioController : BaseController
    {
        private readonly HttpClient _httpClient;

        public UsuarioController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Vista para login
        public IActionResult Login() => View();

        // Procesar login
        [HttpPost]
        public async Task<IActionResult> Login(Usuario usuario)
        {
            if (!ModelState.IsValid) return View(usuario);

            var token = await AuthenticateUserAsync(usuario);
            if (string.IsNullOrEmpty(token))
            {
                ModelState.AddModelError("", "Credenciales incorrectas");
                return View(usuario);
            }

            SetSessionAndClaims(token);
            return RedirectToAction("Index", "Home");
        }

        // Cerrar sesión
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());
            return RedirectToAction("Login", "Usuario");
        }

        // Vista para registrar
        public IActionResult Registrar() => View();

        // Registrar usuario
        [HttpPost]
        public async Task<IActionResult> Registrar(Usuario usuario)
        {
            if (!ModelState.IsValid) return View(usuario);

            if (await RegisterUserAsync(usuario))
            {
                return RedirectToAction("Login");
            }

            ModelState.AddModelError("", "Error al registrar el usuario");
            return View(usuario);
        }

        // Método para autenticar usuario en la API
        private async Task<string> AuthenticateUserAsync(Usuario usuario)
        {
            var response = await PostToApiAsync("usuario/Login", usuario);
            return response.IsSuccessStatusCode
                ? await response.Content.ReadAsStringAsync()
                : null;
        }

        // Método para registrar usuario en la API
        private async Task<bool> RegisterUserAsync(Usuario usuario)
        {
            var response = await PostToApiAsync("usuario/Registrar", usuario);
            return response.IsSuccessStatusCode;
        }

        // Método para hacer POST a la API
        private async Task<HttpResponseMessage> PostToApiAsync(string endpoint, object data)
        {
            var jsonContent = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
            return await _httpClient.PostAsync($"https://localhost:7200/api/{endpoint}", jsonContent);
        }

        // Establecer sesión y claims
        private void SetSessionAndClaims(string token)
        {
            var claims = DecodeJwtClaims(token);
            HttpContext.Session.SetString("JwtToken", token);

            var identity = new ClaimsIdentity(claims, "login");
            HttpContext.User = new ClaimsPrincipal(identity);
        }

        // Decodificar JWT para obtener claims
        private IEnumerable<Claim> DecodeJwtClaims(string token)
        {
            var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token);
            return jwtToken.Claims;
        }
    }
}
