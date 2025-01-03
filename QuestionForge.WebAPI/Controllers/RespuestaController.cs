using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestionForge.EntidadesDeNegocio;
using QuestionForge.LogicaDeNegocio;
using System.Security.Claims;

namespace QuestionForge.WebAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class RespuestaController : ControllerBase
    {
        private readonly RespuestaBL _respuestaBL;

        public RespuestaController(RespuestaBL respuestaBL)
        {
            _respuestaBL = respuestaBL;
        }

        // *********************  Método para Responder a una Pregunta  ******************************
        [HttpPost("ResponderPregunta")]
        public async Task<IActionResult> ResponderPregunta([FromBody] Respuesta respuesta)
        {
            if (respuesta == null || string.IsNullOrEmpty(respuesta.Contenido))
            {
                return BadRequest(new { Error = "El contenido de la respuesta es obligatorio." });
            }

            try
            {
                var idUsuarioClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (idUsuarioClaim == null)
                {
                    return Unauthorized(new { Message = "No se pudo identificar al usuario." });
                }

                respuesta.IdUsuario = Convert.ToInt32(idUsuarioClaim.Value);

                var idRespuesta = await _respuestaBL.ResponderPreguntaAsync(respuesta);
                if (idRespuesta == 0)
                {
                    return BadRequest(new { Error = "No se pudo registrar la respuesta." });
                }

                return Ok(new
                {
                    IdRespuesta = idRespuesta,
                    Message = "Respuesta registrada con éxito."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = $"Ocurrió un error: {ex.Message}" });
            }
        }

        // *********************  Método para Obtener Respuestas por Pregunta  ***********************
        [HttpGet("ObtenerRespuestasPorPregunta/{idPregunta}")]
        public async Task<IActionResult> ObtenerRespuestasPorPregunta(int idPregunta)
        {
            if (idPregunta <= 0)
            {
                return BadRequest(new { Error = "El id de la pregunta no es válido." });
            }

            try
            {
                var respuestas = await _respuestaBL.ObtenerRespuestasPorPreguntaAsync(idPregunta);
                if (respuestas == null || !respuestas.Any())
                {
                    return NotFound(new { Message = "No se encontraron respuestas para esta pregunta." });
                }

                return Ok(respuestas);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
    }
}
