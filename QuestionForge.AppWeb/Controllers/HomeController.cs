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
        private readonly ILogger<HomeController> _logger;

        public HomeController(HttpClient httpClient, ILogger<HomeController> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        // ********* Vista principal: lista las preguntas abiertas ********
        public async Task<IActionResult> Index()
        {
            try
            {
                var preguntas = await ObtenerPreguntasAbiertasAsync();

                // ********* Cargar respuestas para cada pregunta ********
                foreach (var pregunta in preguntas)
                {
                    pregunta.Respuestas = await ObtenerRespuestasPorPreguntaAsync(pregunta.Id);
                    _logger.LogInformation($"Respuestas cargadas para la pregunta {pregunta.Id}: {pregunta.Respuestas.Count} respuestas.");

                    // ********* Imprimir cada respuesta en el log ********
                    foreach (var respuesta in pregunta.Respuestas)
                    {
                        _logger.LogInformation($"Respuesta para la pregunta {pregunta.Id}: {respuesta.Contenido}");
                    }
                }

                return View(preguntas);
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = "Ocurrió un error al cargar las preguntas abiertas.";
                return View(new List<Pregunta>()); // Devuelve una lista vacía
            }
        }

        // ********* Vista para crear una nueva pregunta ********
        public IActionResult Create() => View(new Pregunta());

        // ********* Acción para crear una pregunta (POST) ********
        [HttpPost]
        public async Task<IActionResult> Create(Pregunta pregunta)
        {
            if (!ModelState.IsValid)
                return View(pregunta);

            try
            {
                var token = HttpContext.Session.GetString("JwtToken");
                if (string.IsNullOrEmpty(token))
                    return View(pregunta);

                var crearPreguntaRequest = new
                {
                    Titulo = pregunta.Titulo,
                    Descripcion = pregunta.Descripcion
                };

                // ********* Serializar el contenido de la pregunta a JSON ********
                var jsonContent = new StringContent(JsonConvert.SerializeObject(crearPreguntaRequest), Encoding.UTF8, "application/json");
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://localhost:7200/api/Pregunta/CrearPregunta")
                {
                    Content = jsonContent
                };
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.SendAsync(requestMessage);

                if (response.IsSuccessStatusCode)
                    return RedirectToAction("Index");

                ViewData["ErrorMessage"] = "Error al crear la pregunta.";
                return View(pregunta);
            }
            catch
            {
                ViewData["ErrorMessage"] = "Ocurrió un error.";
                return View(pregunta);
            }
        }

        // ********* Acción para cerrar una pregunta ********
        [HttpPost]
        public async Task<IActionResult> Cerrar(int id)
        {
            var cerrada = await CerrarPreguntaAsync(id);
            if (cerrada)
                return RedirectToAction("Index");

            return RedirectToAction("Index");
        }


        // ********* Obtener las preguntas abiertas desde la API *****************************
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

        // ********* Obtener una pregunta por su ID ********************************************
        private async Task<Pregunta> ObtenerPreguntaPorIdAsync(int idPregunta)
        {
            var response = await _httpClient.GetAsync($"https://localhost:7200/api/Pregunta/{idPregunta}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Pregunta>(json);
            }
            return null;
        }

        // ********* Cerrar una pregunta por su ID ************************************************
        private async Task<bool> CerrarPreguntaAsync(int idPregunta)
        {
            var response = await _httpClient.PostAsync($"https://localhost:7200/api/Pregunta/CerrarPregunta/{idPregunta}", null);
            return response.IsSuccessStatusCode;
        }

        // ********* Obtener respuestas para una pregunta por su ID ********************************
        private async Task<List<Respuesta>> ObtenerRespuestasPorPreguntaAsync(int idPregunta)
        {
            var token = HttpContext.Session.GetString("JwtToken");

            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning($"Token no encontrado. No se puede autenticar la solicitud para obtener respuestas de la pregunta {idPregunta}.");
                return new List<Respuesta>();  // No se puede realizar la solicitud si no se tiene un token
            }

            // ********* Crear la solicitud con el encabezado de autorización **********************
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"https://localhost:7200/api/Respuesta/ObtenerRespuestasPorPregunta/{idPregunta}");
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(requestMessage);

            _logger.LogInformation($"Llamada a la API para obtener respuestas de la pregunta {idPregunta}: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Respuestas obtenidas para la pregunta {idPregunta}: {json}");

                // ********* Deserializar y devolver las respuestas ********
                return JsonConvert.DeserializeObject<List<Respuesta>>(json);
            }

            _logger.LogWarning($"Error al obtener respuestas para la pregunta {idPregunta}: {response.StatusCode}");
            return new List<Respuesta>();
        }
    }
}
