using EMISSOR_DE_CERTIFICADOS.DBConnections;
using EMISSOR_DE_CERTIFICADOS.Interfaces;
using System.Data;
using System.Data.SqlClient;

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
            try
            {
                var query = $"SELECT ID, NOME, IMAGEM_CERTIFICADO FROM EVENTO WHERE ID_USUARIO_ADMINISTRATIVO = {userId}";
                return await _dbHelper.ExecuteQueryAsync(query);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [OrganizadorRepository.BuscarTodosEventosAsync]: {ex.Message}");
            }            
        }
        public async Task<DataTable> BuscarEventoPorIdAsync(int id)
        {
            try
            {
                var sSQL = $"SELECT * FROM EVENTO WHERE ID = {id}";
                return await _dbHelper.ExecuteQueryAsync(sSQL);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [OrganizadorRepository.BuscarEventoPorIdAsync]: {ex.Message}");
            }            
        }
        public async Task<int?> InserirEventoAsync(string nome, byte[] imagemCertificado, int idUsuario)
        {
            try
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
            catch (Exception ex)
            {
                throw new Exception($"Erro em [OrganizadorRepository.InserirEventoAsync]: {ex.Message}");
            }            
        }
        public async Task InserirEventoPessoaAsync(int idEvento, int idPessoa, string texto)
        {
            try
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
            catch (Exception ex)
            {
                throw new Exception($"Erro em [OrganizadorRepository.InserirEventoPessoaAsync]: {ex.Message}");
            }            
        }
        public async Task AtualizarTextoFrenteEventoPessoaAsync(int idEvento, int idPessoa, string texto)
        {
            try
            {
                var sSQL = $"UPDATE EVENTO_PESSOA SET TEXTO_FRENTE = '{texto}' WHERE ID_EVENTO = {idEvento} AND ID_PESSOA = {idPessoa}";
                await _dbHelper.ExecuteQueryAsync(sSQL);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [OrganizadorRepository.AtualizarTextoFrenteEventoPessoaAsync]: {ex.Message}");
            }            
        }
        public async Task<byte[]> BuscarBytesDaImagemNoBDAsync(int id)
        {
            try
            {
                string sql = "SELECT IMAGEM_CERTIFICADO FROM EVENTO WHERE ID = @Id";
                return await _dbHelper.ExecuteQueryArrayBytesAsync(sql, id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [OrganizadorRepository.BuscarBytesDaImagemNoBDAsync]: {ex.Message}");
            }            
        }        
        public async Task<DataTable> ObterEmailConfigAsync()
        {
            try
            {
                var sSQL = "SELECT SMTP, PORTA, EMAIL, SENHA FROM EMAIL_CONFIG";
                return await _dbHelper.ExecuteQueryAsync(sSQL);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [OrganizadorRepository.ObterEmailConfigAsync]: {ex.Message}");
            }           
        }
        public async Task<DataTable> BuscarPessoasEventoAsync(int eventoId, string idPessoas)
        {

            try
            {
                string sSQL = $"SELECT * FROM EVENTO_PESSOA WHERE (CERTIFICADO_EMITIDO IS NULL OR CERTIFICADO_EMITIDO = 0) " +
                              $"AND ID_EVENTO = {eventoId} AND ID_PESSOA IN ({idPessoas})";
                return await _dbHelper.ExecuteQueryAsync(sSQL);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [OrganizadorRepository.BuscarPessoasEventoAsync]: {ex.Message}");
            }
            
        }
        public async Task AtualizarCertificadoEmitidoAsync(int idEventoPessoa, bool certificadoEmitido, string mensagemRetorno)
        {
            string sSQL = string.Empty;
            string dataEmissao = string.Empty;

            try
            {              
                sSQL = "UPDATE EVENTO_PESSOA SET CERTIFICADO_EMITIDO = @CertificadoEmitido ";
                if (certificadoEmitido)
                {
                    sSQL += ", DATA_EMISSAO = GETDATE() ";
                }
                sSQL += ", MENSAGEM_RETORNO_EMAIL = @MensagemRetorno";
                sSQL += " WHERE ID = @IdEventoPessoa";

                var parameters = new Dictionary<string, object>
                {
                    { "@CertificadoEmitido", certificadoEmitido},
                    {"@MensagemRetorno", mensagemRetorno},
                    { "@IdEventoPessoa", idEventoPessoa }
                };

                await _dbHelper.ExecuteQueryAsync(sSQL, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [OrganizadorRepository.AtualizarCertificadoEmitidoAsync]: {ex.Message}");
            }
        }
    }
}