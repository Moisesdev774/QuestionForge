using Microsoft.AspNetCore.Mvc;
using QuestionForge.EntidadesDeNegocio;
using QuestionForge.LogicaDeNegocio;

namespace QuestionForge.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioController : ControllerBase
    {
        private readonly UsuarioBL _usuarioBL;

        public UsuarioController(UsuarioBL usuarioBL)
        {
            _usuarioBL = usuarioBL;
        }

        [HttpGet("ObtenerTodos")]
        public async Task<IActionResult> ObtenerTodosLosUsuarios()
        {
            try
            {
                var usuarios = await _usuarioBL.ObtenerTodosLosUsuariosAsync();
                return Ok(usuarios);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("Registrar")]
        public IActionResult RegistrarUsuario([FromBody] Usuario usuario)
        {
            try
            {
                _usuarioBL.RegistrarUsuarioAsync(usuario.Nombre, usuario.Password);
                return Ok("Usuario registrado con éxito.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("Login")]
        public async Task<IActionResult> VerificarCredenciales([FromBody] Usuario usuario)
        {
            try
            {
                var resultado = await _usuarioBL.VerificarCredencialesAsync(usuario.Nombre, usuario.Password);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }
    }
}
