using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuestionForge.AccesoADatos;
using QuestionForge.EntidadesDeNegocio;

namespace QuestionForge.LogicaDeNegocio
{
    public class RespuestaBL
    {
        private readonly RespuestaDAL _respuestaDAL;

        public RespuestaBL(string connectionString)
        {
            _respuestaDAL = new RespuestaDAL(connectionString);
        }

        // *********************  Lógica para Responder a una Pregunta  ***************************
        public async Task<int> ResponderPreguntaAsync(Respuesta respuesta)
        {
            if (respuesta == null)
                throw new ArgumentNullException(nameof(respuesta));

            if (string.IsNullOrWhiteSpace(respuesta.Contenido))
                throw new ArgumentException("La respuesta no puede estar vacía.");

            return await _respuestaDAL.ResponderPreguntaAsync(respuesta);
        }

        // *********************  Lógica para Obtener Respuestas Por Pregunta  ***************************

        public async Task<List<Respuesta>> ObtenerRespuestasPorPreguntaAsync(int idPregunta)
        {
            return await _respuestaDAL.ObtenerRespuestasPorPreguntaAsync(idPregunta);
        }

    }
}
