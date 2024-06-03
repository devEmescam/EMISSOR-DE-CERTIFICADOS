using System.Data;
using System.Data.SqlClient;

namespace EMISSOR_DE_CERTIFICADOS.DBConnections
{
    public class DBHelpers
    {
        private readonly Dictionary<string, string> _connectionStrings;
        public DBHelpers(Dictionary<string, string> connectionStrings)
        {
            _connectionStrings = connectionStrings ?? throw new ArgumentNullException(nameof(connectionStrings), "O dicionário de cadeias de conexão não pode ser nulo.");
        }
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

        // Sobrecarga do método ExecuteQuery para aceitar parâmetros
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

    }
}
