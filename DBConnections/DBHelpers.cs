using System.Data;
using System.Data.SqlClient;

namespace EMISSOR_DE_CERTIFICADOS.DBConnections
{
    public class DBHelpers : IDBHelpers
    {
        private readonly Dictionary<string, string> _connectionStrings;
        public DBHelpers(Dictionary<string, string> connectionStrings)
        {
            _connectionStrings = connectionStrings ?? throw new ArgumentNullException(nameof(connectionStrings), "O dicionário de cadeias de conexão não pode ser nulo.");
        }

        //Busca a string de conexão do sistema
        public IDbConnection GetConnection(string connectionName = "CertificadoConnection")
        {
            if (!_connectionStrings.TryGetValue(connectionName, out var connectionString))
            {
                throw new ArgumentException($"Não foi possivel encontrar a string de conexão '{connectionName}.");
            }

            IDbConnection connection = new SqlConnection(connectionString);
            connection.Open();

            return connection;
        }
        //VERSÃO ASYNC: Busca a string de conexão do sistema
        public async Task<IDbConnection> GetConnectionAsync(string connectionName = "CertificadoConnection")
        {
            try
            {
                if (!_connectionStrings.TryGetValue(connectionName, out var connectionString))
                {
                    throw new ArgumentException($"Não foi possível encontrar a string de conexão '{connectionName}'.");
                }

                IDbConnection connection = new SqlConnection(connectionString);
                await ((SqlConnection)connection).OpenAsync();

                return connection;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ocorreu um erro em [DBHelpers.GetConnectionAsync] Erro: {ex.Message}");
            }            
        }

        //Executa a consulta retornado um datatable do resultado
        public DataTable ExecuteQuery(string query, string connectionName = "CertificadoConnection")
        {
            using (var connection = GetConnection(connectionName))
            {
                using (var command = new SqlCommand(query, (SqlConnection)connection))
                {
                    using (var adapter = new SqlDataAdapter(command))
                    {
                        var dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        return dataTable;
                    }
                }
            }
        }
        //VERSÃO ASYNC: Executa a consulta retornado um datatable do resultado
        public async Task<DataTable> ExecuteQueryAsync(string query, string connectionName = "CertificadoConnection")
        {
            try
            {
                using (var connection = GetConnection(connectionName))
                {
                    using (var command = new SqlCommand(query, (SqlConnection)connection))
                    {
                        using (var adapter = new SqlDataAdapter(command))
                        {
                            var dataTable = new DataTable();
                            await Task.Run(() => adapter.Fill(dataTable));
                            return dataTable;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ocorreu um erro em [DBHelpers.ExecuteQueryAsync] Erro: {ex.Message}");
            }            
        }

        //Sobrecarga do método ExecuteQuery para aceitar parâmetros
        public DataTable ExecuteQuery(string query, Dictionary<string, object> parameters, string connectionName = "CertificadoConnection")
        {
            using (var connection = GetConnection(connectionName))
            {
                using (var command = new SqlCommand(query, (SqlConnection)connection))
                {
                    foreach (var param in parameters)
                    {
                        command.Parameters.AddWithValue(param.Key, param.Value);
                    }

                    using (var adapter = new SqlDataAdapter(command))
                    {
                        var dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        return dataTable;
                    }
                }
            }
        }
        //VERSÃO ASYNC: Sobrecarga do método ExecuteQuery para aceitar parâmetros
        public async Task<DataTable> ExecuteQueryAsync(string query, Dictionary<string, object> parameters, string connectionName = "CertificadoConnection")
        {
            using (var connection = await GetConnectionAsync(connectionName))
            {
                using (var command = new SqlCommand(query, (SqlConnection)connection))
                {
                    foreach (var param in parameters)
                    {
                        command.Parameters.AddWithValue(param.Key, param.Value);
                    }

                    using (var adapter = new SqlDataAdapter(command))
                    {
                        var dataTable = new DataTable();
                        await Task.Run(() => adapter.Fill(dataTable));
                        return dataTable;
                    }
                }
            }
        }
        
        //Executa a consulta retornando um array de bytes
        public byte[] ExecuteQueryArrayBytes(string query, int id, string connectionName = "CertificadoConnection")
        {
            try
            {
                using (var connection = GetConnection(connectionName))
                {
                    using (var command = new SqlCommand(query, (SqlConnection)connection))
                    {
                        // Crie um SqlParameter para o parâmetro @Id
                        var idParameter = new SqlParameter("@Id", SqlDbType.Int);
                        idParameter.Value = id;

                        command.Parameters.Add(idParameter);

                        // Execute o comando e obtenha o resultado
                        object result = command.ExecuteScalar();

                        // Verifique se o resultado é nulo ou DBNull
                        if (result == null || result == DBNull.Value)
                        {
                            return null; // Retorna nulo se a imagem não existir
                        }
                        else
                        {
                            // Converta o resultado em um array de bytes e retorne
                            return (byte[])result;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ocorreu um erro em [DBHelpers.ExecuteQueryArrayBytes] Erro: {ex.Message}");
            }
        }
        //VERSÃO ASYNC: Executa a consulta retornando um array de bytes
        public async Task<byte[]> ExecuteQueryArrayBytesAsync(string query, int id, string connectionName = "CertificadoConnection")
        {
            try
            {
                using (var connection = await GetConnectionAsync(connectionName))
                {
                    using (var command = new SqlCommand(query, (SqlConnection)connection))
                    {
                        // Crie um SqlParameter para o parâmetro @Id
                        var idParameter = new SqlParameter("@Id", SqlDbType.Int);
                        idParameter.Value = id;

                        command.Parameters.Add(idParameter);

                        // Execute o comando e obtenha o resultado
                        var result = await command.ExecuteScalarAsync();

                        // Verifique se o resultado é nulo ou DBNull
                        if (result == null || result == DBNull.Value)
                        {
                            return null; // Retorna nulo se a imagem não existir
                        }
                        else
                        {
                            // Converta o resultado em um array de bytes e retorne
                            return (byte[])result;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ocorreu um erro em [DBHelpers.ExecuteQueryArrayBytes] Erro: {ex.Message}");
            }
        }
        
        //Executa a consulta retornando um objeto
        public object ExecuteScalar(string query, string connectionName = "CertificadoConnection")
        {
            using (var connection = GetConnection(connectionName))
            {
                using (var command = new SqlCommand(query, (SqlConnection)connection))
                {
                    return command.ExecuteScalar();
                }
            }
        }
        //VERSÃO ASYNC: Executa a consulta retornando um objeto
        public async Task<object> ExecuteScalarAsync(string query, string connectionName = "CertificadoConnection")
        {
            using (var connection = await GetConnectionAsync(connectionName))
            {
                using (var command = new SqlCommand(query, (SqlConnection)connection))
                {
                    return await command.ExecuteScalarAsync();
                }
            }
        }

        //Executa a consulta e retorna o resultado com o tipo de dado informado na chamado
        public T ExecuteScalar<T>(string query, string connectionName = "CertificadoConnection")
        {
            using (var connection = GetConnection(connectionName))
            {
                using (var command = new SqlCommand(query, (SqlConnection)connection))
                {
                    var result = command.ExecuteScalar();

                    // Converte o resultado para o tipo T especificado
                    return (result == DBNull.Value) ? default(T) : (T)Convert.ChangeType(result, typeof(T));
                }
            }
        }

        //VERSÃO ASYNC: Executa a consulta e retorna o resultado com o tipo de dado informado na chamado
        public async Task<T> ExecuteScalarAsync<T>(string query, string connectionName = "CertificadoConnection")
        {
            using (var connection = await GetConnectionAsync(connectionName))
            {
                using (var command = new SqlCommand(query, (SqlConnection)connection))
                {
                    var result = await command.ExecuteScalarAsync();

                    // Converte o resultado para o tipo T especificado
                    return (result == DBNull.Value) ? default(T) : (T)Convert.ChangeType(result, typeof(T));
                }
            }
        }

        //Sobrecarga do metodo: Executa a consulta, recebendo como parametro um array de bytes e retorna o resultado com o tipo de dado informado na chamado
        public T ExecuteScalar<T>(string query, Dictionary<string, object> parameters, string connectionName = "CertificadoConnection")
        {
            using (var connection = GetConnection(connectionName))
            {
                using (var command = new SqlCommand(query, (SqlConnection)connection))
                {
                    foreach (var param in parameters)
                    {
                        command.Parameters.AddWithValue(param.Key, param.Value);
                    }

                    var result = command.ExecuteScalar();

                    // Converte o resultado para o tipo T especificado
                    return (result == DBNull.Value) ? default(T) : (T)Convert.ChangeType(result, typeof(T));
                }
            }
        }

        //VERSÃO ASYNC: Sobrecarga do metodo: Executa a consulta, recebendo como parametro um array de bytes e retorna o resultado com o tipo de dado informado na chamado
        public async Task<T> ExecuteScalarAsync<T>(string query, Dictionary<string, object> parameters, string connectionName = "CertificadoConnection")
        {
            using (var connection = await GetConnectionAsync(connectionName))
            {
                using (var command = new SqlCommand(query, (SqlConnection)connection))
                {
                    foreach (var param in parameters)
                    {
                        command.Parameters.AddWithValue(param.Key, param.Value);
                    }

                    var result = await command.ExecuteScalarAsync();

                    // Converte o resultado para o tipo T especificado
                    return (result == DBNull.Value) ? default(T) : (T)Convert.ChangeType(result, typeof(T));
                }
            }
        }
    }
}