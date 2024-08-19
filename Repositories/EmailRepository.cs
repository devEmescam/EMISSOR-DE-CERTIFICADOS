using EMISSOR_DE_CERTIFICADOS.DBConnections;
using EMISSOR_DE_CERTIFICADOS.Interfaces;

namespace EMISSOR_DE_CERTIFICADOS.Repositories
{
    internal class EmailRepository : IEmailRepository
    {
        private readonly IDBHelpers _dbHelper;

        public EmailRepository(IDBHelpers dBHelper)
        {
            _dbHelper = dBHelper ?? throw new ArgumentNullException(nameof(dBHelper), "O IDBHelpers não pode ser nulo.");
        }
        public async Task<string> RetornarNomeEventoAsync(int idEventoPessoa)
        {
            string sSQL = string.Empty;
            string retorno  = string.Empty;
            try
            {
                sSQL = "SELECT E.NOME FROM EVENTO_PESSOA EP" +
                      " JOIN EVENTO E ON (EP.ID_EVENTO = E.ID)" +
                      " WHERE EP.ID = @IdEventoPessoa";

                var parameters = new Dictionary<string, object>
                {
                    { "@IdEventoPessoa", idEventoPessoa }
                };               

                retorno = await _dbHelper.ExecuteScalarAsync<string>(sSQL, parameters);
                if (string.IsNullOrEmpty(retorno))
                {
                    throw new Exception("Nenhum nomeEvento encontrado para o idEventoPessoa fornecido.");
                }
                return retorno;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [EmailRepository.RetornarNomeEventoAsync]: {ex.Message}");
            }
        }
        public async Task<string> RetornarNomePessoaAsync(int idEventoPessoa)
        {
            string sSQL = string.Empty;
            string retorno = string.Empty;

            try
            {
                sSQL = "SELECT P.NOME FROM EVENTO_PESSOA EP" +
                     " JOIN PESSOA P ON (EP.ID_PESSOA = P.ID)" +
                     " WHERE EP.ID = @IdEventoPessoa";

                var parameters = new Dictionary<string, object>
                {
                    { "@IdEventoPessoa", idEventoPessoa }
                };

                retorno = await _dbHelper.ExecuteScalarAsync<string>(sSQL, parameters);

                if (string.IsNullOrEmpty(retorno))
                {
                    throw new Exception("Nenhum nomePessoa encontrado para o idEventoPessoa fornecido.");
                }

                return retorno;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [EmailRepository.RetornarNomePessoaAsync]: {ex.Message}");
            }
        }
        public async Task<string> RetornarEmailPessoaAsync(int idEventoPessoa)
        {
            string sSQL = string.Empty;
            string retorno = string.Empty;

            try
            {

                sSQL = "SELECT P.EMAIL FROM EVENTO_PESSOA EP" +
                    " JOIN PESSOA P ON (EP.ID_PESSOA = P.ID)" +
                    " WHERE EP.ID = @IdEventoPessoa";

                var parameters = new Dictionary<string, object>
                {
                    { "@IdEventoPessoa", idEventoPessoa }
                };

                retorno = await _dbHelper.ExecuteScalarAsync<string>(sSQL, parameters);

                if (string.IsNullOrEmpty(retorno))
                {
                    throw new Exception("Nenhum emailPessoa encontrado para o idEventoPessoa fornecido.");
                }

                return retorno;

            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [EmailRepository.RetornarEmailPessoaAsync]: {ex.Message}");
            }
        }
        public async Task<byte[]> RetornarCertificadoAsync(int idEventoPessoa)
        {
            try
            {                
                string sql = "SELECT IMAGEM_CERTIFICADO FROM EVENTO_PESSOA WHERE ID = @Id";

                byte[] imagemBytes = await _dbHelper.ExecuteQueryArrayBytesAsync(sql, idEventoPessoa);

                return imagemBytes;

            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [EmailRepository.RetornarCertificadoAsync]: {ex.Message}");
            }
        }
    }
}
