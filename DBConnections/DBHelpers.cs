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
    }
}
