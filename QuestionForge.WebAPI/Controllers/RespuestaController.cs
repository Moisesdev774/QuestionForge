using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuestionForge.EntidadesDeNegocio;
using QuestionForge.LogicaDeNegocio;

namespace QuestionForge.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RespuestaController : ControllerBase
    {
        private readonly RespuestaBL _respuestaBL;

        public RespuestaController(RespuestaBL respuestaBL)
        {
            _respuestaBL = respuestaBL;
        }

        [HttpPost("ResponderPregunta")]
        public async Task<IActionResult> ResponderPregunta([FromBody] Respuesta respuesta)
        {
            try
            {
                var idRespuesta = await _respuestaBL.ResponderPreguntaAsync(respuesta);
                return Ok(new { IdRespuesta = idRespuesta, Message = "Respuesta registrada con éxito." });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("ObtenerRespuestasPorPregunta/{idPregunta}")]
        public async Task<IActionResult> ObtenerRespuestasPorPregunta(int idPregunta)
        {
            try
            {
                var respuestas = await _respuestaBL.ObtenerRespuestasPorPreguntaAsync(idPregunta);
                return Ok(respuestas);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}
