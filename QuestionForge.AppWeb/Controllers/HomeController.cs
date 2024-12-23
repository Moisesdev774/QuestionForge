using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Newtonsoft.Json;
using QuestionForge.EntidadesDeNegocio;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

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

        // Vista principal para listar preguntas abiertas
        public async Task<IActionResult> Index()
        {
            try
            {
                var preguntas = await ObtenerPreguntasAbiertasAsync();

                // Cargar respuestas para cada pregunta
                foreach (var pregunta in preguntas)
                {
                    pregunta.Respuestas = await ObtenerRespuestasPorPreguntaAsync(pregunta.Id);
                }

                return View(preguntas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar preguntas abiertas.");
                ViewData["ErrorMessage"] = "Ocurrió un error al cargar las preguntas abiertas.";
                return View(new List<Pregunta>()); // Devuelve una lista vacía
            }
        }


        // Vista para crear una nueva pregunta
        public IActionResult Create()
        {
            return View(new Pregunta());
        }

        // Crear nueva pregunta (POST)
        [HttpPost]
        public async Task<IActionResult> Create(Pregunta pregunta)
        {
            if (!ModelState.IsValid)
            {
                ViewData["ErrorMessage"] = "Los datos proporcionados no son válidos.";
                return View(pregunta);
            }

            // Asegúra de que los datos obligatorios estén completos
            if (pregunta.IdUsuario == 0 || string.IsNullOrEmpty(pregunta.Titulo) || string.IsNullOrEmpty(pregunta.Descripcion))
            {
                ViewData["ErrorMessage"] = "Por favor complete todos los campos requeridos.";
                return View(pregunta);
            }

            try
            {
                // Llamada a la API para crear la pregunta
                var result = await CrearPreguntaAsync(pregunta);
                if (result)
                {
                    TempData["SuccessMessage"] = "Pregunta creada exitosamente.";
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewData["ErrorMessage"] = "Hubo un problema al crear la pregunta.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear pregunta.");
                ViewData["ErrorMessage"] = "Ocurrió un error al crear la pregunta.";
            }
            return View(pregunta);
        }

        // Acción para cerrar una pregunta
        [HttpPost]
        public async Task<IActionResult> Cerrar(int id)
        {
            try
            {
                var cerrada = await CerrarPreguntaAsync(id);
                if (cerrada)
                {
                    TempData["SuccessMessage"] = "La pregunta se cerró exitosamente.";
                }
                else
                {
                    TempData["ErrorMessage"] = "No se pudo cerrar la pregunta.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al cerrar la pregunta con ID {id}.");
                TempData["ErrorMessage"] = "Ocurrió un error al intentar cerrar la pregunta.";
            }
            return RedirectToAction("Index");
        }

        // Métodos auxiliares
        private async Task<List<Pregunta>> ObtenerPreguntasAbiertasAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("https://localhost:7200/api/Pregunta/ListarPreguntasAbiertas");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<Pregunta>>(json);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener preguntas abiertas.");
            }
            return new List<Pregunta>();
        }

        private async Task<bool> CrearPreguntaAsync(Pregunta pregunta)
        {
            try
            {
                var jsonContent = new StringContent(JsonConvert.SerializeObject(pregunta), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("https://localhost:7200/api/Pregunta/CrearPregunta", jsonContent);
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<dynamic>(jsonResponse);
                    return result?.IdPregunta != null; // Verifica si se creó la pregunta
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear pregunta.");
            }
            return false;
        }

        private async Task<bool> CerrarPreguntaAsync(int idPregunta)
        {
            try
            {
                var response = await _httpClient.PostAsync($"https://localhost:7200/api/Pregunta/CerrarPregunta/{idPregunta}", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al cerrar la pregunta con ID {idPregunta}.");
            }
            return false;
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
        [HttpPost]
        public async Task<IActionResult> Responder(int IdPregunta, string Contenido)
        {
            if (string.IsNullOrEmpty(Contenido))
            {
                TempData["ErrorMessage"] = "La respuesta no puede estar vacía.";
                return RedirectToAction("Index");
            }

            try
            {
                var respuesta = new Respuesta
                {
                    IdPregunta = IdPregunta,
                    Contenido = Contenido,
                    NombreUsuario = "UsuarioActual", // Cambia esto según cómo obtengo el usuario actual
                    FechaCreacion = DateTime.Now
                };

                var jsonContent = new StringContent(JsonConvert.SerializeObject(respuesta), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("https://localhost:7200/api/Respuesta/CrearRespuesta", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Respuesta enviada exitosamente.";
                }
                else
                {
                    TempData["ErrorMessage"] = "No se pudo enviar la respuesta.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar la respuesta.");
                TempData["ErrorMessage"] = "Ocurrió un error al enviar la respuesta.";
            }

            return RedirectToAction("Index");
        }


    }
}
