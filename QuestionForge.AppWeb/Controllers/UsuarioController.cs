using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using QuestionForge.AppWeb.Models;
using System.Security.Claims;

namespace QuestionForge.AppWeb.Controllers
{
    // ********* Clase base para centralizar la autorización ********
    public class BaseController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // ********* Excluyo acciones específicas de autorización ********
            var skipAuthActions = new[] { "Login", "Registrar" };
            var currentAction = context.RouteData.Values["action"]?.ToString();

            if (!skipAuthActions.Contains(currentAction))
            {
                var usuarioLogeado = context.HttpContext.Session.GetString("UsuarioLogeado");
                if (string.IsNullOrEmpty(usuarioLogeado))
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

        // ********* Vista para iniciar sesión ********
        public IActionResult Login()
        {
            return View();
        }

        // ********* Acción para procesar el login ********
        [HttpPost]
        public async Task<IActionResult> Login(Usuario usuario)
        {
            if (ModelState.IsValid)
            {
                var token = await VerificarCredencialesAsync(usuario);
                if (!string.IsNullOrEmpty(token))
                {
                    // ********* Decodifico el JWT y obtengo las claims ********
                    var claims = DecodeJwtClaims(token);

                    // Guardamos el token y las claims en la sesión
                    var identity = new ClaimsIdentity(claims, "login");
                    var userPrincipal = new ClaimsPrincipal(identity);
                    HttpContext.User = userPrincipal; // Asigno las claims al HttpContext.User

                    HttpContext.Session.SetString("JwtToken", token); // Guardo el token en la sesión

                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError("", "Credenciales incorrectas");
            }
            return View(usuario);
        }

        // ********* Acción para cerrar sesión ********
        public IActionResult Logout()
        {
            // Limpio la sesión y las claims
            HttpContext.Session.Clear();
            HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity()); // Limpio el principal
            return RedirectToAction("Login", "Usuario");
        }

        // ********* Vista para el registro de usuarios ********
        public IActionResult Registrar()
        {
            return View();
        }

        // ********* Acción para registrar un nuevo usuario ********
        [HttpPost]
        public async Task<IActionResult> Registrar(Usuario usuario)
        {
            if (ModelState.IsValid)
            {
                var registroResponse = await RegistrarUsuarioAsync(usuario);
                if (registroResponse)
                {
                    // Redirijo al login después de un registro exitoso
                    return RedirectToAction("Login");
                }
                ModelState.AddModelError("", "Hubo un error al registrar el usuario.");
            }
            return View(usuario);
        }

        // ********* Verifico las credenciales del usuario contra la API ********
        private async Task<string> VerificarCredencialesAsync(Usuario usuario)
        {
            var jsonContent = new StringContent(JsonConvert.SerializeObject(usuario), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("https://localhost:7200/api/usuario/Login", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                return result; // El JWT token es devuelto directamente
            }
            return null;
        }

        // ********* Decodifico el JWT y extraigo las claims ********
        private IEnumerable<Claim> DecodeJwtClaims(string token)
        {
            var jwtHandler = new JwtSecurityTokenHandler();
            var jwtToken = jwtHandler.ReadJwtToken(token);

            var claims = jwtToken.Claims.ToList();
            return claims;
        }

        // ********* Registro un nuevo usuario en la API ********
        private async Task<bool> RegistrarUsuarioAsync(Usuario usuario)
        {
            var jsonContent = new StringContent(JsonConvert.SerializeObject(usuario), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("https://localhost:7200/api/usuario/Registrar", jsonContent);
            return response.IsSuccessStatusCode;
        }
    }
}
