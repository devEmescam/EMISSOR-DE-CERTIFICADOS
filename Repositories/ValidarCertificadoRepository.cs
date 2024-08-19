using EMISSOR_DE_CERTIFICADOS.Interfaces;
using EMISSOR_DE_CERTIFICADOS.DBConnections;
using System.Data;

namespace EMISSOR_DE_CERTIFICADOS.Repositories
{
    internal class ValidarCertificadoRepository: IValidarCertificadoRepository
    {
        private readonly IDBHelpers _dbHelpers;
        public ValidarCertificadoRepository(IDBHelpers dBHelpers)
        {
            _dbHelpers = dBHelpers;
        }
        public async Task<bool> ValidarCodigoCertificado(string codigo)
        {
			try
			{
                var sSQL = $"SELECT COUNT(*) FROM EVENTO_PESSOA WHERE CODIGO_CERTIFICADO = '{codigo}'";
                var result = await _dbHelpers.ExecuteScalarAsync<int>(sSQL);
                int count = Convert.ToInt32(result);
                return count > 0;
            }
			catch (Exception ex)
			{
                throw new Exception($"Ocorreu erro em [ValidarCertificadoRepository.ValidarCodigoCertificado]. Erro: {ex.Message}");
            }
        }

        public async Task<DataTable> ObterImagemCertificadoPorCodigo(string codigo)
        {
            try
            {
                var query = $"SELECT IMAGEM_CERTIFICADO FROM EVENTO_PESSOA WHERE CODIGO_CERTIFICADO = '{codigo}'";
                return await _dbHelpers.ExecuteQueryAsync(query);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [ValidarCertificadoRepository.ObterImagemCertificadoPorCodigo]: {ex.Message}");
            }
        }

    }
}
