using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using QuestionForge.EntidadesDeNegocio;

namespace QuestionForge.AccesoADatos
{
    public class PreguntaDAL
    {
        private readonly string _connectionString;

        public PreguntaDAL(string connectionString)
        {
            _connectionString = connectionString;
        }

        // *********************  Método Asíncrono para Crear una Nueva Pregunta  ***************************
        public async Task<int> CrearPreguntaAsync(Pregunta pregunta)
        {
            await using (var connection = new SqlConnection(_connectionString))
            {
                await using (var command = new SqlCommand("SP_CrearPregunta", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@IdUsuario", pregunta.IdUsuario);
                    command.Parameters.AddWithValue("@Titulo", pregunta.Titulo);
                    command.Parameters.AddWithValue("@Descripcion", pregunta.Descripcion);

                    await connection.OpenAsync();
                    return Convert.ToInt32(await command.ExecuteScalarAsync());
                }
            }
        }

        // *********************  Método Asíncrono para Listar Preguntas Abiertas  ***************************
        public async Task<List<Pregunta>> ListarPreguntasAbiertasAsync()
        {
            var preguntas = new List<Pregunta>();
            await using (var connection = new SqlConnection(_connectionString))
            {
                await using (var command = new SqlCommand("SP_ListarPreguntasAbiertas", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    await connection.OpenAsync();
                    await using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            preguntas.Add(new Pregunta
                            {
                                Id = reader["Id"] as int? ?? 0,
                                IdUsuario = reader["IdUsuario"] as int? ?? 0,
                                NombreUsuario = reader["NombreUsuario"]?.ToString(),
                                Titulo = reader["Titulo"]?.ToString(),
                                Descripcion = reader["Descripcion"]?.ToString(),
                                FechaCreacion = reader["FechaCreacion"] as DateTime? ?? DateTime.MinValue,
                                Cerrada = false
                            });
                        }
                    }
                }
            }
            return preguntas;
        }

        // *********************  Método Asíncrono para Cerrar una Pregunta  ***************************
        public async Task CerrarPreguntaAsync(int idPregunta)
        {
            await using (var connection = new SqlConnection(_connectionString))
            {
                await using (var command = new SqlCommand("SP_CerrarPregunta", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@IdPregunta", idPregunta);

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        // *********************  Método Asíncrono para Listar Preguntas Cerradas  ***************************
        public async Task<List<Pregunta>> ListarPreguntasCerradasAsync()
        {
            var preguntas = new List<Pregunta>();
            await using (var connection = new SqlConnection(_connectionString))
            {
                await using (var command = new SqlCommand("SP_ListarPreguntasCerradas", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    await connection.OpenAsync();
                    await using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            preguntas.Add(new Pregunta
                            {
                                Id = reader["Id"] as int? ?? 0,
                                IdUsuario = reader["IdUsuario"] as int? ?? 0,
                                NombreUsuario = reader["NombreUsuario"]?.ToString(),
                                Titulo = reader["Titulo"]?.ToString(),
                                Descripcion = reader["Descripcion"]?.ToString(),
                                FechaCreacion = reader["FechaCreacion"] as DateTime? ?? DateTime.MinValue,
                                Cerrada = true
                            });
                        }
                    }
                }
            }
            return preguntas;
        }

       

    }
}
