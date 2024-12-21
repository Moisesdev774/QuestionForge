using System;
using System.Threading.Tasks;
using QuestionForge.AccesoADatos;
using QuestionForge.EntidadesDeNegocio;

namespace QuestionForge.LogicaDeNegocio
{
    public class UsuarioBL
    {
        private readonly UsuarioDAL _usuarioDAL;

        public UsuarioBL(string connectionString)
        {
            _usuarioDAL = new UsuarioDAL(connectionString);
        }
        
        // *************************  Logica del Metodo Registrar Usuario  ***************************
        public async Task RegistrarUsuarioAsync(string nombre, string password)
        {
            
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre no puede estar vacío");
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("La contraseña no puede estar vacía");

            await _usuarioDAL.RegistrarUsuarioAsync(nombre, password); 
        }

        // **********************  Logica del Metodo Validar Credenciales Usuario  ********************
        public async Task<Usuario> VerificarCredencialesAsync(string nombre, string password)
        {
            if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Nombre y contraseña son obligatorios");

            return await _usuarioDAL.VerificarCredencialesAsync(nombre, password); 
        }

        // **********************  Lógica del Método para Obtener Todos los Usuarios ********************
        public async Task<List<Usuario>> ObtenerTodosLosUsuariosAsync()
        {
            return await _usuarioDAL.ObtenerTodosAsync();
        }

    }
}
