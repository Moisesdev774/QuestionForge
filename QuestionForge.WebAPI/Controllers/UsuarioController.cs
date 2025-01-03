using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestionForge.EntidadesDeNegocio;
using QuestionForge.LogicaDeNegocio;
using QuestionForge.WebAPI.Auth;

namespace QuestionForge.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioController : ControllerBase
    {
        private readonly UsuarioBL _usuarioBL;
        private readonly IJwtAuthenticationService _jwtAuthService;

        public UsuarioController(UsuarioBL usuarioBL, IJwtAuthenticationService jwtAuthService)
        {
            _usuarioBL = usuarioBL;
            _jwtAuthService = jwtAuthService;
        }

        // *********************  Método para Obtener Todos los Usuarios  ******************************
        [HttpGet("ObtenerTodos")]
        [Authorize]
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

        // *********************  Método para Registrar un Nuevo Usuario  ******************************
        [HttpPost("Registrar")]
        public async Task<IActionResult> RegistrarUsuario([FromBody] Usuario usuario)
        {
            try
            {
                await _usuarioBL.RegistrarUsuarioAsync(usuario.Nombre, usuario.Password);
                return Ok("Usuario registrado con éxito.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // *********************  Método para Login y Autenticación con JWT  ****************************
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] Usuario usuario)
        {
            try
            {
                var usuarioAuth = await _usuarioBL.VerificarCredencialesAsync(usuario.Nombre, usuario.Password);

                if (usuarioAuth == null || usuarioAuth.Id <= 0)
                {
                    return Unauthorized("Credenciales incorrectas.");
                }

                var token = _jwtAuthService.Authenticate(usuarioAuth);
                return Ok(token);
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }
    }
}
