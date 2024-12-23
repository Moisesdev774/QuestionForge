using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using QuestionForge.EntidadesDeNegocio;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace QuestionForge.Web.Controllers
{
    public class PreguntaController : Controller
    {
        private readonly HttpClient _httpClient;

        public PreguntaController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Vista principal: lista las preguntas abiertas
        public async Task<IActionResult> Index()
        {
            var preguntas = await ObtenerPreguntasAbiertasAsync();
            return View(preguntas);
        }

        // Vista para crear una nueva pregunta
        public IActionResult Create()
        {
            return View(new Pregunta());
        }

        // Acción para crear una pregunta (POST)
        [HttpPost]
        public async Task<IActionResult> Create(Pregunta pregunta)
        {
            if (!ModelState.IsValid || pregunta.IdUsuario == 0 || string.IsNullOrEmpty(pregunta.Titulo) || string.IsNullOrEmpty(pregunta.Descripcion))
            {
                ViewData["ErrorMessage"] = "Por favor complete todos los campos requeridos.";
                return View(pregunta);
            }

            var result = await CrearPreguntaAsync(pregunta);
            if (result)
            {
                return RedirectToAction("Index");
            }

            ViewData["ErrorMessage"] = "Hubo un problema al crear la pregunta.";
            return View(pregunta);
        }

        // Acción para cerrar una pregunta
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

        // Vista de detalles: muestra una pregunta y sus respuestas
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

        // Acción para registrar una respuesta
        [HttpPost]
        public async Task<IActionResult> ResponderPregunta(Respuesta respuesta)
        {
            if (respuesta.IdPregunta <= 0 || respuesta.IdUsuario <= 0 || string.IsNullOrEmpty(respuesta.Contenido))
            {
                ModelState.AddModelError("", "Todos los campos son obligatorios.");
                return RedirectToAction("Detalles", new { id = respuesta.IdPregunta });
            }

            var jsonContent = new StringContent(JsonConvert.SerializeObject(respuesta), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("https://localhost:7200/api/Respuesta/ResponderPregunta", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Respuesta registrada con éxito.";
            }
            else
            {
                TempData["ErrorMessage"] = "No se pudo registrar la respuesta.";
            }

            return RedirectToAction("Detalles", new { id = respuesta.IdPregunta });
        }

        // Métodos auxiliares para interactuar con la API

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

        private async Task<bool> CrearPreguntaAsync(Pregunta pregunta)
        {
            var jsonContent = new StringContent(JsonConvert.SerializeObject(pregunta), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("https://localhost:7200/api/Pregunta/CrearPregunta", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<dynamic>(jsonResponse);
                return result?.IdPregunta != null;
            }
            return false;
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
}
