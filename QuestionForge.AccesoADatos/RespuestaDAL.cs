using Microsoft.Data.SqlClient;
using QuestionForge.EntidadesDeNegocio;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestionForge.AccesoADatos
{
    public class RespuestaDAL
    {
        private readonly string _connectionString;

        public RespuestaDAL(string connectionString)
        {
            _connectionString = connectionString;
        }

        // *********************  Método Asíncrono para Responder Pregunta *******************************

        public async Task<int> ResponderPreguntaAsync(Respuesta respuesta)
        {
            await using (var connection = new SqlConnection(_connectionString))
            {
                await using (var command = new SqlCommand("SP_ResponderPregunta", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@IdPregunta", respuesta.IdPregunta);
                    command.Parameters.AddWithValue("@IdUsuario", respuesta.IdUsuario);
                    command.Parameters.AddWithValue("@Contenido", respuesta.Contenido);

                    await connection.OpenAsync();
                    return Convert.ToInt32(await command.ExecuteScalarAsync());
                }
            }
        }

        // *********************  Método Asíncrono para Obtener Respuesta de Pregunta *******************************

        public async Task<List<Respuesta>> ObtenerRespuestasPorPreguntaAsync(int idPregunta)
        {
            var respuestas = new List<Respuesta>();
            await using (var connection = new SqlConnection(_connectionString))
            {
                await using (var command = new SqlCommand("SP_ObtenerRespuestasPorPregunta", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@IdPregunta", idPregunta);

                    await connection.OpenAsync();
                    await using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            respuestas.Add(new Respuesta
                            {
                                Id = reader.GetInt32(0),
                                IdPregunta = reader.GetInt32(1),
                                IdUsuario = reader.GetInt32(2),
                                Nombre = reader.IsDBNull(3) ? null : reader.GetString(3), 
                                Contenido = reader.GetString(4),
                                FechaCreacion = reader.GetDateTime(5)
                            });
                        }
                    }
                }
            }
            return respuestas;
        }

    }
}
