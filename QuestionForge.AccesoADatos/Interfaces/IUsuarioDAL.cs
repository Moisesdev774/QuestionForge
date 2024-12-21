using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuestionForge.EntidadesDeNegocio;

namespace QuestionForge.AccesoADatos.Interfaces
{
    public interface IUsuarioDAL
    {
        Task RegistrarUsuarioAsync(string nombre, string contrasena);
        Task<Usuario> VerificarCredencialesAsync(string nombre, string contrasena);
    }
}

