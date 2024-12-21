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
        public async Task RegistrarUsuarioAsync(string nombre, string contrasena)
        {
            
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre no puede estar vacío");
            if (string.IsNullOrWhiteSpace(contrasena))
                throw new ArgumentException("La contraseña no puede estar vacía");

            await _usuarioDAL.RegistrarUsuarioAsync(nombre, contrasena); 
        }

        // **********************  Logica del Metodo Validar Credenciales Usuario  ********************

        public async Task<Usuario> VerificarCredencialesAsync(string nombre, string contrasena)
        {
            if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(contrasena))
                throw new ArgumentException("Nombre y contraseña son obligatorios");

            return await _usuarioDAL.VerificarCredencialesAsync(nombre, contrasena); 
        }
    }
}
