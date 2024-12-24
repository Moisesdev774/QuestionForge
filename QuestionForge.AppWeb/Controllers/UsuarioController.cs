using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using QuestionForge.EntidadesDeNegocio;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace QuestionForge.AppWeb.Controllers
{
    // Clase base para centralizar la autorización
    public class BaseController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Excluir acciones específicas de autorización
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

        // Vista para iniciar sesión
        public IActionResult Login()
        {
            return View();
        }

        // Acción para procesar el login
        [HttpPost]
        public async Task<IActionResult> Login(Usuario usuario)
        {
            if (ModelState.IsValid)
            {
                var loginResponse = await VerificarCredencialesAsync(usuario);
                if (loginResponse != null)
                {
                    // Guardar el usuario en la sesión
                    HttpContext.Session.SetString("UsuarioLogeado", JsonConvert.SerializeObject(loginResponse));
                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError("", "Credenciales incorrectas");
            }
            return View(usuario);
        }

        public IActionResult Logout()
        {
            // Limpia la sesión y redirige al login
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Usuario");
        }

        // Vista para el registro de usuarios
        public IActionResult Registrar()
        {
            return View();
        }

        // Acción para registrar un nuevo usuario
        [HttpPost]
        public async Task<IActionResult> Registrar(Usuario usuario)
        {
            if (ModelState.IsValid)
            {
                var registroResponse = await RegistrarUsuarioAsync(usuario);
                if (registroResponse)
                {
                    // Redirige al login después de un registro exitoso
                    return RedirectToAction("Login");
                }
                ModelState.AddModelError("", "Hubo un error al registrar el usuario.");
            }
            return View(usuario);
        }

        // Verifica las credenciales del usuario contra la API
        private async Task<Usuario> VerificarCredencialesAsync(Usuario usuario)
        {
            var jsonContent = new StringContent(JsonConvert.SerializeObject(usuario), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("https://localhost:7200/api/usuario/Login", jsonContent);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Usuario>(result);
            }
            return null;
        }

        // Registra un nuevo usuario en la API
        private async Task<bool> RegistrarUsuarioAsync(Usuario usuario)
        {
            var jsonContent = new StringContent(JsonConvert.SerializeObject(usuario), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("https://localhost:7200/api/usuario/Registrar", jsonContent);
            return response.IsSuccessStatusCode;
        }
    }
}
