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
    public class PreguntaController : Controller
    {
        private readonly HttpClient _httpClient;

        public PreguntaController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("PreguntaAPI");
        }

        // ********* Vista principal: lista las preguntas abiertas ********
        public async Task<IActionResult> Index()
        {
            var preguntas = await ObtenerPreguntasAbiertasAsync();
            return View(preguntas);
        }

        // ********* Vista para crear una nueva pregunta ********
        public IActionResult Create()
        {
            return View(new Pregunta());
        }

        // ********* Acción para crear una pregunta  ********
        [HttpPost]
        public async Task<IActionResult> Create(Pregunta pregunta)
        {
            if (!ModelState.IsValid)
            {
                ViewData["ErrorMessage"] = "Modelo no válido.";

                // ********* Imprimir errores de validación en la consola ********
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Error: {error.ErrorMessage}");
                }

                return View(pregunta);
            }

            try
            {
                var token = HttpContext.Session.GetString("JwtToken");
                if (string.IsNullOrEmpty(token))
                {
                    ViewData["ErrorMessage"] = "Usuario no autenticado.";
                    return View(pregunta);
                }

                // ********* Crear el modelo simplificado para enviar a la API ********
                var crearPreguntaRequest = new CrearPreguntaRequest
                {
                    Titulo = pregunta.Titulo,
                    Descripcion = pregunta.Descripcion
                };

                // ********* Serializar el modelo a JSON ********
                var jsonContent = new StringContent(JsonConvert.SerializeObject(crearPreguntaRequest), Encoding.UTF8, "application/json");
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://localhost:7200/api/Pregunta/CrearPregunta")
                {
                    Content = jsonContent
                };
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // ********* Enviar solicitud HTTP a la API ********
                var response = await _httpClient.SendAsync(requestMessage);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    ViewData["ErrorMessage"] = $"Error: {response.StatusCode}. Detalles: {errorMessage}";
                    return View(pregunta);
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = $"Ocurrió un error: {ex.Message}";
                return View(pregunta);
            }
        }

        // ********* Acción para cerrar una pregunta ********
        [HttpPost]
        public async Task<IActionResult> Cerrar(int id)
        {
            var cerrada = await CerrarPreguntaAsync(id);
            if (cerrada)
            {
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", "No se pudo cerrar la pregunta.");
            return RedirectToAction("Index");
        }

        // ********* Vista de detalles: muestra una pregunta y sus respuestas ********
        public async Task<IActionResult> Detalles(int id)
        {
            try
            {
                var preguntasAbiertas = await ObtenerPreguntasAbiertasAsync();
                var pregunta = preguntasAbiertas.FirstOrDefault(p => p.Id == id);

                if (pregunta == null)
                {
                    return NotFound(); // Error 404 si la pregunta no existe
                }

                var respuestas = await ObtenerRespuestasPorPreguntaAsync(id);
                ViewBag.Respuestas = respuestas;

                return View(pregunta);
            }
            catch
            {
                return BadRequest("Ocurrió un error al cargar los detalles de la pregunta.");
            }
        }

        // ********* Métodos auxiliares para interactuar con la API ********

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

        private async Task<bool> CerrarPreguntaAsync(int idPregunta)
        {
            var response = await _httpClient.PostAsync($"https://localhost:7200/api/Pregunta/CerrarPregunta/{idPregunta}", null);
            return response.IsSuccessStatusCode;
        }

        private async Task<List<Respuesta>> ObtenerRespuestasPorPreguntaAsync(int idPregunta)
        {
            var response = await _httpClient.GetAsync($"https://localhost:7200/api/Respuesta/ObtenerRespuestasPorPregunta/{idPregunta}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<Respuesta>>(json);
            }
            return new List<Respuesta>();
        }
    }

    // ********* Modelo simplificado para la creación de preguntas ********
    public class CrearPreguntaRequest
    {
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
    }
}
