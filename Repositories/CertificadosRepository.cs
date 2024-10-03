using EMISSOR_DE_CERTIFICADOS.DBConnections;
using EMISSOR_DE_CERTIFICADOS.Interfaces;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace EMISSOR_DE_CERTIFICADOS.Repositories
{
    internal class CertificadosRepository : ICertificadosRepository
    {
        private readonly IDBHelpers _dbHelpers;

        public CertificadosRepository(IDBHelpers dBHelpers)
        {
            _dbHelpers = dBHelpers;
        }

        public async Task InserirAsync(int idEventoPessoa, byte[] certificadoBytes, string codigoCertificado)
        {
            try
            {
                string sSQL = "UPDATE EVENTO_PESSOA SET IMAGEM_CERTIFICADO = @Certificado, CODIGO_CERTIFICADO = @CodigoCertificado WHERE ID = @IdEventoPessoa";

                using (var connection = _dbHelpers.GetConnection("CertificadoConnection"))
                {
                    using (var command = new SqlCommand(sSQL, (SqlConnection)connection))
                    {
                        command.Parameters.AddWithValue("@Certificado", certificadoBytes);
                        command.Parameters.AddWithValue("@CodigoCertificado", codigoCertificado);
                        command.Parameters.AddWithValue("@IdEventoPessoa", idEventoPessoa);
                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [CertificadosService.Inserir]: {ex.Message}");
            }
        }

        // Novo método para buscar o certificado pelo ID do evento-pessoa
        public async Task<byte[]> ObterCertificadoPorIdAsync(int idEventoPessoa)
        {
            try
            {
                string sSQL = "SELECT IMAGEM_CERTIFICADO FROM EVENTO_PESSOA WHERE ID = @IdEventoPessoa";

                using (var connection = _dbHelpers.GetConnection("CertificadoConnection"))
                {
                    using (var command = new SqlCommand(sSQL, (SqlConnection)connection))
                    {
                        command.Parameters.AddWithValue("@IdEventoPessoa", idEventoPessoa);

                        var certificado = await command.ExecuteScalarAsync();

                        // Verifica se a consulta retornou um valor
                        if (certificado != null && certificado is byte[])
                        {
                            return (byte[])certificado;
                        }
                        else
                        {
                            throw new Exception("Certificado não encontrado ou inválido.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [CertificadosRepository.ObterCertificadoPorIdAsync]: {ex.Message}");
            }
        }
    }
}
