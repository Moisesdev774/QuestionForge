using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using QuestionForge.EntidadesDeNegocio;

namespace QuestionForge.WebAPI.Auth
{
    public interface IJwtAuthenticationService
    {
        string Authenticate(Usuario usuario); // Método para autenticar al usuario y generar el token
    }
}

