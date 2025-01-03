using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestionForge.EntidadesDeNegocio;
using QuestionForge.LogicaDeNegocio;
using System.Security.Claims;

namespace QuestionForge.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PreguntaController : ControllerBase
    {
        private readonly PreguntaBL _preguntaBL;
        private readonly RespuestaBL _respuestaBL;

        public PreguntaController(PreguntaBL preguntaBL, RespuestaBL respuestaBL)
        {
            _preguntaBL = preguntaBL;
            _respuestaBL = respuestaBL;
        }

        // *********************  Método para Crear una Nueva Pregunta  ******************************
        [HttpPost("CrearPregunta")]
        public async Task<IActionResult> CrearPregunta([FromBody] Pregunta pregunta)
        {
            if (pregunta == null || string.IsNullOrEmpty(pregunta.Titulo) || string.IsNullOrEmpty(pregunta.Descripcion))
            {
                return BadRequest(new { Error = "Los campos Titulo y Contenido son obligatorios." });
            }

            try
            {
                // Obtener el IdUsuario desde las claims del token
                var idUsuarioClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (idUsuarioClaim == null)
                {
                    return Unauthorized(new { Message = "No se pudo identificar al usuario." });
                }

                pregunta.IdUsuario = Convert.ToInt32(idUsuarioClaim.Value);

                // Obtener el NombreUsuario desde las claims
                var nombreUsuarioClaim = User.FindFirst(ClaimTypes.Name);
                pregunta.NombreUsuario = nombreUsuarioClaim?.Value ?? "Usuario desconocido";

                // Crear la pregunta
                var idPregunta = await _preguntaBL.CrearPreguntaAsync(pregunta);
                if (idPregunta == 0)
                {
                    return BadRequest(new { Error = "No se pudo crear la pregunta." });
                }

                return Ok(new
                {
                    IdPregunta = idPregunta,
                    Message = "Pregunta creada con éxito."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = $"Ocurrió un error: {ex.Message}" });
            }
        }

        // *********************  Método para Listar Preguntas Abiertas  ******************************
        [HttpGet("ListarPreguntasAbiertas")]
        public async Task<IActionResult> ListarPreguntasAbiertas()
        {
            try
            {
                var preguntas = await _preguntaBL.ListarPreguntasAbiertasAsync();
                return Ok(preguntas);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        // *********************  Método para Cerrar una Pregunta  ************************************
        [HttpPost("CerrarPregunta/{idPregunta}")]
        public async Task<IActionResult> CerrarPregunta(int idPregunta)
        {
            try
            {
                await _preguntaBL.CerrarPreguntaAsync(idPregunta);
                return Ok(new { Message = "Pregunta cerrada con éxito." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        // *********************  Método para Listar Preguntas Cerradas  ******************************
        [HttpGet("ListarPreguntasCerradas")]
        public async Task<IActionResult> ListarPreguntasCerradas()
        {
            try
            {
                var preguntas = await _preguntaBL.ListarPreguntasCerradasAsync();
                return Ok(preguntas);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
    }
}
