using System;
using System.Data;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using QuestionForge.EntidadesDeNegocio;


namespace QuestionForge.AccesoADatos
{
    public class UsuarioDAL
    {
        private readonly string _connectionString;

        public UsuarioDAL(string connectionString)
        {
            _connectionString = connectionString;
        }

        // *********************  Método Asíncrono para Registrar un Nuevo Usuario  ***************************
        public async Task RegistrarUsuarioAsync(string nombre, string password)
        {
            await using (var connection = new SqlConnection(_connectionString))
            {
                await using (var command = new SqlCommand("SP_RegistrarUsuario", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Nombre", nombre);
                    command.Parameters.AddWithValue("@Password", password);

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync(); 
                }
            }
        }

        // *********************  Método Asíncrono para Validar Credenciales  **********************************
        public async Task<Usuario> VerificarCredencialesAsync(string nombre, string password)
        {
            await using (var connection = new SqlConnection(_connectionString))
            {
                await using (var command = new SqlCommand("SP_VerificarCredenciales", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Nombre", nombre);
                    command.Parameters.AddWithValue("@Password", password);

                    await connection.OpenAsync(); 
                    await using (var reader = await command.ExecuteReaderAsync()) 
                    {
                        if (await reader.ReadAsync()) 
                        {
                            return new Usuario
                            {
                                Id = reader.GetInt32(0),
                                Nombre = reader.GetString(1)
                            };
                        }
                        else
                        {
                            throw new InvalidOperationException("Credenciales incorrectas");
                        }
                    }
                }
            }
        }

        public async Task<List<Usuario>> ObtenerTodosAsync()
        {
            var usuarios = new List<Usuario>(); // Lista para almacenar los usuarios
            await using (var connection = new SqlConnection(_connectionString))
            {
                await using (var command = new SqlCommand("SP_ObtenerTodosLosUsuarios", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    await connection.OpenAsync();
                    await using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            usuarios.Add(new Usuario
                            {
                                Id = reader.GetInt32(0), 
                                Nombre = reader.GetString(1), 
                                Password = reader.GetString(2), 
                                FechaRegistro = reader.GetDateTime(3) 
                            });
                        }
                    }
                }
            }
            return usuarios;
        }
    }
}
