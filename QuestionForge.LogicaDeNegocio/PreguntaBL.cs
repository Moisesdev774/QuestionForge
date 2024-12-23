// Capa de Lógica de Negocio para Preguntas y Respuestas
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using QuestionForge.AccesoADatos;
using QuestionForge.EntidadesDeNegocio;

namespace QuestionForge.LogicaDeNegocio
{
    public class PreguntaBL
    {
        private readonly PreguntaDAL _preguntaDAL;

        public PreguntaBL(string connectionString)
        {
            _preguntaDAL = new PreguntaDAL(connectionString);
        }

        // *********************  Lógica para Crear una Nueva Pregunta  ***************************
        public async Task<int> CrearPreguntaAsync(Pregunta pregunta)
        {
            if (pregunta == null)
                throw new ArgumentNullException(nameof(pregunta));

            if (string.IsNullOrWhiteSpace(pregunta.Titulo))
                throw new ArgumentException("El título de la pregunta no puede estar vacío.");

            if (string.IsNullOrWhiteSpace(pregunta.Descripcion))
                throw new ArgumentException("La descripción de la pregunta no puede estar vacía.");

            return await _preguntaDAL.CrearPreguntaAsync(pregunta);
        }

        // *********************  Lógica para Listar Preguntas Abiertas  ***************************
        public async Task<List<Pregunta>> ListarPreguntasAbiertasAsync()
        {
            return await _preguntaDAL.ListarPreguntasAbiertasAsync();
        }

        // *********************  Lógica para Cerrar una Pregunta  ***************************
        public async Task CerrarPreguntaAsync(int idPregunta)
        {
            if (idPregunta <= 0)
                throw new ArgumentException("El ID de la pregunta no es válido.");

            await _preguntaDAL.CerrarPreguntaAsync(idPregunta);
        }

        // *********************  Lógica para Listar Preguntas Abiertas  ***************************
        public async Task<List<Pregunta>> ListarPreguntasCerradasAsync()
        {
            return await _preguntaDAL.ListarPreguntasCerradasAsync();
        }

       
    }
}
