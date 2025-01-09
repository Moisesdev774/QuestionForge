using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Security.Claims;
using QuestionForge.AppWeb.Models;
using System.Net.Http.Headers;

namespace QuestionForge.AppWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly HttpClient _httpClient;

        public HomeController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // ********* Mostrar la lista principal de preguntas ********
        public async Task<IActionResult> Index()
        {
            var token = HttpContext.Session.GetString("JwtToken");

            // Verificar si el usuario tiene sesión activa
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Usuario");
            }

            try
            {
                // Obtener las preguntas abiertas de la API
                var preguntas = await ObtenerPreguntasAbiertasAsync();

                foreach (var pregunta in preguntas)
                {
                    // Obtener las respuestas para cada pregunta
                    pregunta.Respuestas = await ObtenerRespuestasPorPreguntaAsync(pregunta.Id);
                }

                return View(preguntas);
            }
            catch
            {
                ViewData["ErrorMessage"] = "No se pudieron cargar las preguntas.";
                return View(new List<Pregunta>());
            }
        }

        // ********* Página para crear una pregunta ********
        public IActionResult Create()
        {
            return View(new Pregunta());
        }

        // ********* Acción para guardar una nueva pregunta ********
        [HttpPost]
        public async Task<IActionResult> Create(Pregunta pregunta)
        {
            if (!ModelState.IsValid)
            {
                return View(pregunta);
            }

            var token = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Usuario");
            }

            try
            {
                // Crear el objeto para enviar a la API
                var jsonContent = new StringContent(JsonConvert.SerializeObject(pregunta), Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(HttpMethod.Post, "https://localhost:7200/api/Pregunta/CrearPregunta")
                {
                    Content = jsonContent
                };

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Enviar la solicitud
                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }

                ViewData["ErrorMessage"] = "Error al crear la pregunta.";
                return View(pregunta);
            }
            catch
            {
                ViewData["ErrorMessage"] = "Hubo un error inesperado.";
                return View(pregunta);
            }
        }

        // ********* Acción para cerrar una pregunta ********
        [HttpPost]
        public async Task<IActionResult> Cerrar(int id)
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Usuario");
            }

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, $"https://localhost:7200/api/Pregunta/CerrarPregunta/{id}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }

                ViewData["ErrorMessage"] = "No se pudo cerrar la pregunta.";
                return RedirectToAction("Index");
            }
            catch
            {
                ViewData["ErrorMessage"] = "Error al cerrar la pregunta.";
                return RedirectToAction("Index");
            }
        }

        // ********* Método para obtener preguntas abiertas ********
        private async Task<List<Pregunta>> ObtenerPreguntasAbiertasAsync()
        {
            var response = await _httpClient.GetAsync("https://localhost:7200/api/Pregunta/ListarPreguntasAbiertas");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<Pregunta>>(json);
            }

            return new List<Pregunta>();
        }

        // ********* Método para obtener respuestas por ID de pregunta ********
        private async Task<List<Respuesta>> ObtenerRespuestasPorPreguntaAsync(int idPregunta)
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token))
            {
                return new List<Respuesta>();
            }

            var request = new HttpRequestMessage(HttpMethod.Get, $"https://localhost:7200/api/Respuesta/ObtenerRespuestasPorPregunta/{idPregunta}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<Respuesta>>(json);
            }

            return new List<Respuesta>();
        }
    }
}
