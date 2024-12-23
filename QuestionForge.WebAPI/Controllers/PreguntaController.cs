using Microsoft.AspNetCore.Mvc;
using QuestionForge.EntidadesDeNegocio;
using QuestionForge.LogicaDeNegocio;

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

        [HttpPost("CrearPregunta")]
        public async Task<IActionResult> CrearPregunta([FromBody] Pregunta pregunta)
        {
            try
            {
                var idPregunta = await _preguntaBL.CrearPreguntaAsync(pregunta);
                return Ok(new
                {
                    IdPregunta = idPregunta,
                    Message = "Pregunta creada con éxito."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

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
