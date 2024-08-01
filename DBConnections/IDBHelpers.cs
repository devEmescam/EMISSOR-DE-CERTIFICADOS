using System.Data;

namespace EMISSOR_DE_CERTIFICADOS.DBConnections
{
    public interface IDBHelpers
    {
        IDbConnection GetConnection(string connectionName = "CertificadoConnection");
        Task<IDbConnection> GetConnectionAsync(string connectionName = "CertificadoConnection");
        DataTable ExecuteQuery(string query, string connectionName = "CertificadoConnection");
        Task<DataTable> ExecuteQueryAsync(string query, string connectionName = "CertificadoConnection");
        DataTable ExecuteQuery(string query, Dictionary<string, object> parameters, string connectionName = "CertificadoConnection");
        Task<DataTable> ExecuteQueryAsync(string query, Dictionary<string, object> parameters, string connectionName = "CertificadoConnection");
        byte[] ExecuteQueryArrayBytes(string query, int id, string connectionName = "CertificadoConnection");
        Task<byte[]> ExecuteQueryArrayBytesAsync(string query, int id, string connectionName = "CertificadoConnection");
        object ExecuteScalar(string query, string connectionName = "CertificadoConnection");
        Task<object> ExecuteScalarAsync(string query, string connectionName = "CertificadoConnection");
        T ExecuteScalar<T>(string query, string connectionName = "CertificadoConnection");
        Task<T> ExecuteScalarAsync<T>(string query, string connectionName = "CertificadoConnection");
        T ExecuteScalar<T>(string query, Dictionary<string, object> parameters, string connectionName = "CertificadoConnection");
        Task<T> ExecuteScalarAsync<T>(string query, Dictionary<string, object> parameters, string connectionName = "CertificadoConnection");
    }
}