using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
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

        public async Task<IActionResult> Index()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken")))
            {
                return RedirectToAction("Login", "Usuario");
            }

            try
            {
                var preguntas = await ObtenerPreguntasAbiertasAsync();
                return View(preguntas);
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = $"Error al cargar preguntas: {ex.Message}";
                return View(new List<Pregunta>());
            }
        }

        public IActionResult Create()
        {
            return View(new Pregunta());
        }

        [HttpPost]
        public async Task<IActionResult> Create(Pregunta pregunta)
        {
            if (!ModelState.IsValid)
            {
                ViewData["ErrorMessage"] = "Modelo inválido.";
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
                    ViewData["ErrorMessage"] = "Token no encontrado.";
                    return View(pregunta);
                }

                var crearPreguntaRequest = new CrearPreguntaRequest
                {
                    Titulo = pregunta.Titulo,
                    Descripcion = pregunta.Descripcion
                };

                var jsonContent = new StringContent(JsonConvert.SerializeObject(crearPreguntaRequest), Encoding.UTF8, "application/json");
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://localhost:7200/api/Pregunta/CrearPregunta")
                {
                    Content = jsonContent
                };
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.SendAsync(requestMessage);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    ViewData["ErrorMessage"] = $"Error al crear la pregunta: {response.StatusCode}. Detalles: {errorMessage}";
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

        [HttpPost]
        public async Task<IActionResult> Cerrar(int id)
        {
            try
            {
                var cerrado = await CerrarPreguntaAsync(id);
                if (cerrado)
                {
                    return RedirectToAction("Index");
                }
                ModelState.AddModelError("", "No se pudo cerrar la pregunta.");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = $"Error: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        public async Task<IActionResult> Detalles(int id)
        {
            try
            {
                var preguntasAbiertas = await ObtenerPreguntasAbiertasAsync();
                var pregunta = preguntasAbiertas.FirstOrDefault(p => p.Id == id);

                if (pregunta == null)
                {
                    return NotFound();
                }

                var respuestas = await ObtenerRespuestasPorPreguntaAsync(id);
                ViewBag.Respuestas = respuestas;

                return View(pregunta);
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = $"Error al cargar detalles: {ex.Message}";
                return View();
            }
        }

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

    public class CrearPreguntaRequest
    {
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
    }
}