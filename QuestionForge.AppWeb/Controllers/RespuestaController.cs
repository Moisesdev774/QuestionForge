using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using QuestionForge.AppWeb.Models;
using System.Net.Http.Headers;

namespace QuestionForge.AppWeb.Controllers
{
    public class RespuestaController : Controller
    {
        private readonly HttpClient _httpClient;

        public RespuestaController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("PreguntaAPI");
        }

        // ********* Acción para mostrar la lista de respuestas de todas las preguntas ********
        public async Task<IActionResult> Index()
        {
            try
            {
                var respuestas = await ObtenerRespuestasPorPreguntaAsync(); // Obtengo las respuestas
                return View(respuestas);
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = $"Ocurrió un error al cargar las respuestas: {ex.Message}";
                return View(new List<Respuesta>());
            }
        }

        // ********* Acción para ver las respuestas específicas de una pregunta ********
        public async Task<IActionResult> Detalles(int idPregunta)
        {
            try
            {
                var respuestas = await ObtenerRespuestasPorPreguntaAsync(idPregunta);

                var modelo = new Pregunta
                {
                    Id = idPregunta,
                    Respuestas = respuestas ?? new List<Respuesta>()
                };

                return View(modelo);
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = $"Ocurrió un error: {ex.Message}";
                return View(new Pregunta { Id = idPregunta, Respuestas = new List<Respuesta>() });
            }
        }

        // ********* Acción para responder a una pregunta ********
        [HttpPost]
        public async Task<IActionResult> Responder(int idPregunta, string contenido)
        {
            if (string.IsNullOrEmpty(contenido))
            {
                TempData["ErrorMessage"] = "El contenido de la respuesta es obligatorio.";
                return RedirectToAction("Detalles", "Pregunta", new { idPregunta });
            }

            try
            {
                var token = HttpContext.Session.GetString("JwtToken");
                if (string.IsNullOrEmpty(token))
                {
                    TempData["ErrorMessage"] = "Usuario no autenticado.";
                    return RedirectToAction("Detalles", "Pregunta", new { idPregunta });
                }

                var respuestaRequest = new Respuesta
                {
                    Contenido = contenido,
                    IdPregunta = idPregunta
                };

                var jsonContent = new StringContent(JsonConvert.SerializeObject(respuestaRequest), Encoding.UTF8, "application/json");
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://localhost:7200/api/Respuesta/ResponderPregunta")
                {
                    Content = jsonContent
                };
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.SendAsync(requestMessage);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Respuesta registrada con éxito.";
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    TempData["ErrorMessage"] = $"Error al registrar la respuesta: {errorMessage}";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Ocurrió un error: {ex.Message}";
            }

            return RedirectToAction("Index", "Home", new { idPregunta });
        }

        // ********* Método auxiliar para obtener respuestas de una pregunta específica ********
        private async Task<List<Respuesta>> ObtenerRespuestasPorPreguntaAsync(int idPregunta = 0)
        {
            var endpoint = idPregunta > 0
                ? $"https://localhost:7200/api/Respuesta/ObtenerRespuestasPorPregunta/{idPregunta}"
                : "https://localhost:7200/api/Respuesta/ObtenerRespuestasPorPregunta";

            var response = await _httpClient.GetAsync(endpoint);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<Respuesta>>(json);
            }

            return new List<Respuesta>();
        }
    }
}
