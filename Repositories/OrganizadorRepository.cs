using EMISSOR_DE_CERTIFICADOS.DBConnections;
using EMISSOR_DE_CERTIFICADOS.Interfaces;
using System.Data;

namespace EMISSOR_DE_CERTIFICADOS.Repositories
{
    internal class OrganizadorRepository : IOrganizadorRepository
    {

        private readonly IDBHelpers _dbHelper;

        public OrganizadorRepository(IDBHelpers dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public async Task<DataTable> BuscarTodosEventosAsync(int userId)
        {
            var query = $"SELECT ID, NOME, IMAGEM_CERTIFICADO FROM EVENTO WHERE ID_USUARIO_ADMINISTRATIVO = {userId}";
            return await _dbHelper.ExecuteQueryAsync(query);
        }

        public async Task<DataTable> BuscarEventoPorIdAsync(int id)
        {
            var sSQL = $"SELECT * FROM EVENTO WHERE ID = {id}";
            return await _dbHelper.ExecuteQueryAsync(sSQL);
        }

        public async Task<int?> InserirEventoAsync(string nome, byte[] imagemCertificado, int idUsuario)
        {
            var sSQL = "INSERT INTO EVENTO (NOME, IMAGEM_CERTIFICADO, ID_USUARIO_ADMINISTRATIVO, DATA_CADASTRO) " +
                       "VALUES (@Nome, @ImagemCertificado, @IdUsuarioAdministrativo, GETDATE()); " +
                       "SELECT SCOPE_IDENTITY();";

            var parameters = new Dictionary<string, object>
            {
                { "@Nome", nome },
                { "@ImagemCertificado", imagemCertificado },
                { "@IdUsuarioAdministrativo", idUsuario }
            };

            return await _dbHelper.ExecuteScalarAsync<int>(sSQL, parameters);
        }

        public async Task InserirEventoPessoaAsync(int idEvento, int idPessoa, string texto)
        {
            var sSQL = "INSERT INTO EVENTO_PESSOA (ID_EVENTO, ID_PESSOA, TEXTO_FRENTE) " +
                       "VALUES (@idEvento, @idPessoa, @texto)";

            var parametersEP = new Dictionary<string, object>
            {
                { "@idEvento", idEvento },
                { "@idPessoa", idPessoa },
                { "@texto", texto }
            };

            await _dbHelper.ExecuteQueryAsync(sSQL, parametersEP);
        }

        public async Task AtualizarTextoFrenteEventoPessoaAsync(int idEvento, int idPessoa, string texto)
        {
            var sSQL = $"UPDATE EVENTO_PESSOA SET TEXTO_FRENTE = '{texto}' WHERE ID_EVENTO = {idEvento} AND ID_PESSOA = {idPessoa}";
            await _dbHelper.ExecuteQueryAsync(sSQL);
        }

        public async Task<byte[]> BuscarBytesDaImagemNoBDAsync(int id)
        {
            string sql = "SELECT IMAGEM_CERTIFICADO FROM EVENTO WHERE ID = @Id";
            return await _dbHelper.ExecuteQueryArrayBytesAsync(sql, id);
        }

        public async Task InserirCertificadoAsync(int idEvento, int idPessoa)
        {
            var sSQL = "INSERT INTO CERTIFICADO (ID_EVENTO, ID_PESSOA, DATA_EMISSAO) " +
                       "VALUES (@idEvento, @idPessoa, GETDATE());";

            var parameters = new Dictionary<string, object>
            {
                { "@idEvento", idEvento },
                { "@idPessoa", idPessoa }
            };

            await _dbHelper.ExecuteQueryAsync(sSQL, parameters);
        }

        public async Task<DataTable> ObterEmailConfigAsync()
        {
            var sSQL = "SELECT SMTP, PORTA, EMAIL, SENHA FROM EMAIL_CONFIG";
            return await _dbHelper.ExecuteQueryAsync(sSQL);
        }

    }
}
